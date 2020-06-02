using System.Text.RegularExpressions;
using Unity.UIWidgets.foundation;

namespace Unity.Messenger
{
    public partial class Utils
    {
        static readonly Regex LetterOrNumberRegex = new Regex(@"^[A-Za-z0-9]+$");
        public static string ExtractName(string name)
        {
            var avatarName = "";
            name = name.Trim();
            string[] nameList = Regex.Split(input: name, @"\s{1,}");
            if (nameList.Length > 0) {
                for (int i = 0; i < nameList.Length; i++) {
                    if (i == 2) {
                        break;
                    }

                    var str = nameList[i].ToCharArray();
                    if (i == 0) {
                        avatarName += str.first();
                        if (!LetterOrNumberRegex.IsMatch(str.first().ToString())) {
                            break;
                        }
                    }

                    if (i == 1) {
                        if (LetterOrNumberRegex.IsMatch(str.first().ToString())) {
                            avatarName += str.first();
                        }
                    }
                }
            }

            avatarName = avatarName.ToUpper();
            return avatarName;
        }
    }
}