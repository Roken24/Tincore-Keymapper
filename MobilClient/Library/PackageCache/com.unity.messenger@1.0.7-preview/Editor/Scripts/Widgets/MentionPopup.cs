using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Messenger.Models;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.widgets;
using UnityEngine;
using WebSocketSharp;
using Color = Unity.UIWidgets.ui.Color;


namespace Unity.Messenger.Widgets
{
    public class MentionPopup : StatefulWidget
    {
        private static readonly GlobalKey<MentionPopupState> Key =
            new GlobalObjectKey<MentionPopupState>("mention-popup");

        internal readonly Dictionary<string, User> users;
        internal readonly Dictionary<string, List<ChannelMember>> members;
        internal readonly Dictionary<string, bool> hasMoreMembers;
        internal readonly Func<string> selectedChannelIdGetter;

        public MentionPopup(
            Dictionary<string, User> users,
            Dictionary<string, List<ChannelMember>> members,
            Dictionary<string, bool> hasMoreMembers,
            Func<string> selectedChannelIdGetter) : base(key: Key)
        {
            this.users = users;
            this.members = members;
            this.hasMoreMembers = hasMoreMembers;
            this.selectedChannelIdGetter = selectedChannelIdGetter;
        }

        internal static MentionPopupState currentState
        {
            get { return Key.currentState; }
        }

        public override State createState()
        {
            return new MentionPopupState();
        }
    }

    internal class MentionPopupState : State<MentionPopup>
    {
        internal bool m_Focusing;
        internal bool m_Mentioning;
        private string m_Query;
        private int m_Cursor;
        private List<string> m_SearchResults;

        private int beginIndex = -1;
        private int endIndex = -1;
        internal int selectedIndex = -1;
        private int candidateCount = -1;

        public void UpdateQueryAndCursor(string sz, int cursor)
        {
            m_Query = sz;
            m_Cursor = cursor;
            DetectMention();
            setState();
        }

        public void SelectNext()
        {
            if (!isMentioning)
            {
                return;
            }
            ++selectedIndex;
            if (candidateCount > 0)
            {
                selectedIndex %= candidateCount;
            }

            setState();
        }

        public void SelectPrev()
        {
            if (!isMentioning)
            {
                return;
            }
            if (selectedIndex == -1)
            {
                selectedIndex = 0;
            }
            --selectedIndex;
            selectedIndex += candidateCount;
            if (candidateCount > 0)
            {
                selectedIndex %= candidateCount;
            }
            setState();
        }

        public bool Select()
        {
            String id = null;
            if (candidateCount == 0)
            {
                return false;
            }
            if (beginIndex != -1 && beginIndex == endIndex - 1)
            {
                id = widget.members[widget.selectedChannelIdGetter()]
                    .Where(member => member.user.id != Window.currentUserId)
                    .ToList()[selectedIndex].user.id;
            }
            else
            {
                id = m_SearchResults
                    .Where(uid => uid != Window.currentUserId)
                    .Take(10).ToList()[selectedIndex];
            }

            Sender.currentState?.AddCandidate(
                id,
                m_Query,
                beginIndex,
                endIndex);
            return true;
        }

        private void DetectMention()
        {
            if (!m_Focusing || m_Query.IsNullOrEmpty() || m_Cursor == -1)
            {
                beginIndex = -1;
                endIndex = -1;
                m_Mentioning = false;
                return;
            }

            var currText = m_Query.Substring(0, m_Cursor);
            var query = "";
            var mentioning = false;
            for (var pos = m_Cursor - 1; pos >= 0; pos--)
            {
                if (currText[pos] != '@') continue;
                beginIndex = pos;
                endIndex = m_Cursor;
                query = ((pos + 1) >= currText.Length) ? "" : m_Query.Substring(pos + 1, m_Cursor - pos - 1);
                mentioning = true;
                break;
            }

            var channelId = widget.selectedChannelIdGetter();

            m_SearchResults.Clear();
            var i = 0;
            if (widget.members.ContainsKey(channelId))
            {
                foreach (var member in widget.members[channelId])
                {
                    if (member.user.fullName.ToLower().Contains(query.ToLower()) &&
                        member.user.id != Window.currentUserId)
                    {
                        m_SearchResults.Add(member.user.id);
                        ++i;
                        if (i > 9)
                        {
                            break;
                        }
                    }
                }
            }

            if (!widget.hasMoreMembers.ContainsKey(channelId))
            {
                widget.hasMoreMembers[channelId] = true;
            }
            if (m_SearchResults.Count < 10 && widget.hasMoreMembers[channelId])
            {
                var offset = widget.members[channelId].Count;
                Utils.Get<GetMembersResponse>(
                    $"/api/connectapp/channels/{channelId}/members?offset={offset}"
                ).Then(response =>
                {
                    response.list.ForEach(member =>
                    {
                        if (widget.members[channelId].All(m => m.user.id != member.user.id))
                        {
                            widget.members[channelId].Add(member);
                        }

                        widget.users.putIfAbsent(member.user.id, () => member.user);
                    });
                    
                    widget.hasMoreMembers[channelId] = response.total > widget.members[channelId].Count;
                    if (mounted)
                    {
                        using (WindowProvider.of(context).getScope())
                        {
                            DetectMention();
                            setState();
                        }
                    }
                });
            }

            selectedIndex = -1;
            m_Mentioning = mentioning;
        }

        public void StartMention()
        {
            setState(() => { m_Focusing = true; });
        }

        public void StopMention()
        {
            setState(() => { m_Focusing = false; });
        }

        public override void initState()
        {
            base.initState();
            m_SearchResults = new List<string>();
        }

        public Action onMentionUserSelectedFactory(User user)
        {
            return () => { Sender.currentState?.AddCandidate(user.id, m_Query, beginIndex, endIndex); };
        }

        public bool isMentioning
        {
            get
            {
                if (!m_Focusing || m_Cursor == -1 || !m_Mentioning)
                {
                    return false;
                }

                if (beginIndex != -1 && beginIndex == endIndex - 1)
                {
                    return widget.members[widget.selectedChannelIdGetter()]
                               .Any(member => member.user.id != Window.currentUserId) ||
                           widget.members[widget.selectedChannelIdGetter()].Any(member =>
                               member.user.id == Window.currentUserId && member.role == "admin");
                }

                return m_SearchResults.Count > 0;
            }
        }

        public override Widget build(BuildContext context)
        {
            if (!m_Focusing || m_Cursor == -1 || !m_Mentioning)
            {
                return new Container();
            }

            var children = new List<Widget> { };

            if (beginIndex != -1 && beginIndex == endIndex - 1)
            {
                var selectedChannelId = widget.selectedChannelIdGetter();
                var widgets = widget.members[selectedChannelId]
                    .Where(member => member.user.id != Window.currentUserId)
                    .Take(10)
                    .Select(member => widget.users[member.user.id])
                    .Select((user, idx) => new MentionUser(user,
                        onMentionUserSelectedFactory(user),
                        idx == selectedIndex));
                children.AddRange(
                    widgets
                );
                candidateCount = widgets.Count();
                if (widget.members[widget.selectedChannelIdGetter()].Any(member =>
                    member.user.id == Window.currentUserId && member.role == "admin"))
                {
                    children.Add(
                        new MentionEveryone(
                            onTap: () =>
                            {
                                Sender.currentState?.AddMentionEveryone(m_Query, beginIndex, endIndex);
                                SchedulerBinding.instance.addPostFrameCallback(value =>
                                {
                                    using (WindowProvider.of(context).getScope())
                                    {
                                        StopMention();
                                    }
                                });
                            }
                        )
                    );
                }
            }
            else
            {
                var widgets = m_SearchResults
                    .Select(uid => widget.users[uid])
                    .Select((user, idx) => new MentionUser(
                        user,
                        onMentionUserSelectedFactory(user),
                        idx == selectedIndex));
                children.AddRange(
                    widgets
                );
                candidateCount = widgets.Count();
            }

            if (children.Count == 0)
            {
                return new Container();
            }

            var left = MediaQuery.of(context).size.width < 750 ? 25 : 400;
            return new Positioned(
                bottom: 96,
                left: left,
                child: new Container(
                    padding: EdgeInsets.symmetric(vertical: 6),
                    width: 280,
                    decoration: new BoxDecoration(
                        borderRadius: BorderRadius.all(3),
                        color: new Color(0xffffffff),
                        boxShadow: new List<BoxShadow>
                        {
                            new BoxShadow(
                                blurRadius: 16,
                                color: new Color(0x33000000)
                            )
                        }
                    ),
                    child: new Column(
                        children: children
                    )
                )
            );
        }
    }
}