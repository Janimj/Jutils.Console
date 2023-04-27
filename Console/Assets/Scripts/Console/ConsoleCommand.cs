using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JUtils.Console
{
    [CreateAssetMenu(fileName = "ConsoleCommand", menuName = "Console Command", order = 1), System.Serializable]
    public class ConsoleCommand : ScriptableObject
    {
        [SerializeField] private string[] _keywords;
        [SerializeField] private CommandType _command;
        [SerializeField] private string _description;
        [SerializeField] private bool _includeInHelp = true;                        
        public CommandType Command => _command;
        public string Keyword => _keywords[0].ToLower();
        public string Description => _description;
        public bool Help => _includeInHelp;
        public bool IsValid(string keyword)
        {
            for(int i = 0; i < _keywords.Length; i++)
            {
                if (_keywords[i] == keyword)
                    return true;
            }
            return false;
        }
    }

}
