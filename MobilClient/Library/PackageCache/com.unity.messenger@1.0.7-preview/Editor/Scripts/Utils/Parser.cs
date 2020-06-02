using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;
using UnityEngine.UI;
using Color = Unity.UIWidgets.ui.Color;
using FontStyle = Unity.UIWidgets.ui.FontStyle;

namespace Unity.Messenger
{
    public partial class Utils
    {
        private static readonly Regex BoldRegex = new Regex("\\*\\*(.*?)\\*\\*");
        private static readonly Regex ItalicRegex = new Regex("_(.*?)_");
        private static readonly Regex DeletedRegex = new Regex("~~(.*?)~~");
        private static readonly Regex CodeRegex = new Regex("```(.*?)```");
        private static readonly Regex MentionedRegex = new Regex("<@([0-9a-f]{24})>");

        private static readonly Regex UrlRegex =
            new Regex(
                @"(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_=]*)?");

        private static readonly TextStyle DefaultTextStyle = new TextStyle(
            fontSize: 16,
            color: new Color(0xff000000),
            fontFamily: "PingFang"
        );

        public static TextSpan ParseMessage(
            string text,
            Dictionary<string, Models.User> users,
            TextStyle textStyle = null,
            Action<TapUpDetails> defaultOnTapUp = null
        )
        {
            List<TextSpan> children = new List<TextSpan>();

            var matches = UrlRegex.Matches(text);

            if (matches.Count == 0)
            {
                children.AddRange(ParseOnlyMention(users, text));
            }
            else
            {
                var start = 0;
                foreach (Match match in matches)
                {
                    children.AddRange(
                        ParseOnlyMention(users, text.Substring(start, match.Index - start))
                    );
                    start = match.Index + match.Length;
                    children.Add(
                        new TextSpan(
                            text: match.Value,
                            style: new TextStyle(
                                color: new Color(0xff2196f3),
                                fontFamily: "PingFang"
                            ),
                            recognizer: new TapGestureRecognizer
                            {
                                onTapUp = details =>
                                {
                                    defaultOnTapUp?.Invoke(details);
                                    if (details.kind == PointerDeviceKind.mouse &&
                                        details.device == InputUtils.MouseLeftKeyDevice)
                                    {
                                        Launch(match.Value);
                                    }
                                }
                            }
                        )
                    );
                }

                children.AddRange(ParseOnlyMention(users, text.Substring(start)));
            }

            return new TextSpan(
                children: children,
                style: textStyle ?? DefaultTextStyle
            );
        }

        public static string ParseMessageToString(
            string text,
            Dictionary<string, Models.User> users)
        {
            var escaped = text.Replace("@everyone", "@所有人");
            escaped = ParseMessageToStringInternal(BoldRegex, escaped);
            escaped = ParseMessageToStringInternal(ItalicRegex, escaped);
            escaped = ParseMessageToStringInternal(DeletedRegex, escaped);
            escaped = ParseMessageToStringInternal(CodeRegex, escaped);
            escaped = ParseMentionToString(escaped, users);
            return escaped;
        }

        private static string ParseMessageToStringInternal(Regex regex, string sz)
        {
            var spans = regex.Split(sz);
            return string.Join(string.Empty, spans);
        }

        private static List<TextSpan> ParseOnlyMention(Dictionary<string, Models.User> users, string sz)
        {
            var escaped = sz.Replace("@everyone", "@所有人");
            escaped = ParseMessageToStringInternal(BoldRegex, escaped);
            escaped = ParseMessageToStringInternal(ItalicRegex, escaped);
            escaped = ParseMessageToStringInternal(DeletedRegex, escaped);
            escaped = ParseMessageToStringInternal(CodeRegex, escaped);
            if (MentionedRegex.IsMatch(sz))
            {
                return ParseMentionInternal(users, escaped);
            }

            return new List<TextSpan>
            {
                new TextSpan(text: escaped),
            };
        }

        private static List<TextSpan> ParseRecursive(Dictionary<string, Models.User> users, string sz)
        {
            if (BoldRegex.IsMatch(sz))
            {
                return ParseInternal(users, BoldRegex, sz, new TextStyle(fontWeight: FontWeight.bold));
            }

            if (ItalicRegex.IsMatch(sz))
            {
                return ParseInternal(users, ItalicRegex, sz, new TextStyle(fontStyle: FontStyle.italic));
            }

            if (DeletedRegex.IsMatch(sz))
            {
                return ParseInternal(users, DeletedRegex, sz, new TextStyle(
                    decoration: TextDecoration.lineThrough));
            }

            if (CodeRegex.IsMatch(sz))
            {
                return ParseInternal(users, CodeRegex, sz, new TextStyle(color: new Color(0xffff0000)));
            }

            if (MentionedRegex.IsMatch(sz))
            {
                return ParseMentionInternal(users, sz);
            }

            return new List<TextSpan>
            {
                new TextSpan(text: sz),
            };
        }

        private static List<TextSpan> ParseInternal(
            Dictionary<string, Models.User> users,
            Regex regex, string sz,
            TextStyle matchedStyle)
        {
            if (!regex.IsMatch(sz))
            {
                return new List<TextSpan>
                {
                    new TextSpan(text: sz)
                };
            }

            var spans = regex.Split(sz);
            var children = new List<TextSpan>();

            for (var i = 0; i < spans.Length; ++i)
            {
                children.Add(
                    new TextSpan(
                        children: ParseRecursive(users, spans[i]),
                        style: i % 2 == 1 ? matchedStyle : null
                    )
                );
            }

            return children;
        }

        private static string ParseMentionToString(
            string sz,
            Dictionary<string, Models.User> users
        )
        {
            if (!MentionedRegex.IsMatch(sz))
            {
                return sz;
            }

            var spans = MentionedRegex.Split(sz);
            var children = new List<string>();
            for (var i = 0; i < spans.Length; ++i)
            {
                if (i % 2 == 1)
                {
                    if (users.ContainsKey(spans[i].Trim()))
                    {
                        var user = users[spans[i].Trim()];
                        children.Add(
                            $"@{user.fullName}"
                        );
                    }
                }
                else
                {
                    children.Add(
                        spans[i]
                    );
                }
            }

            return string.Join("", children);
        }

        private static List<TextSpan> ParseMentionInternal(
            Dictionary<string, Models.User> users,
            string sz)
        {
            if (!MentionedRegex.IsMatch(sz))
            {
                return new List<TextSpan>
                {
                    new TextSpan(text: sz)
                };
            }

            var spans = MentionedRegex.Split(sz);
            var children = new List<TextSpan>();

            for (var i = 0; i < spans.Length; ++i)
            {
                if (i % 2 == 1)
                {
                    if (users.ContainsKey(spans[i].Trim()))
                    {
                        var user = users[spans[i].Trim()];
                        children.Add(
                            new TextSpan(
                                text: $"@{user.fullName}"
                            )
                        );
                    }
                }
                else
                {
                    children.Add(
                        new TextSpan(
                            children: ParseRecursive(users, spans[i])
                        )
                    );
                }
            }

            return children;
        }
    }
}