using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JUtils.Console
{
    public class Console : MonoBehaviour
    {
        // Console actions
        public static Action<string[]> Trigger;
        public static Action<string[], Vector2> Vec2;
        public static Action<string[], Vector3> Vec3;
        public static Action<string[], Vector4> Vec4;
        public static Action<string[], float> Float;
        public static Action<string[], int> Int;

        // Console history
        private Queue<string> _history = new Queue<string>();
        private Queue<string> _debug = new Queue<string>();

        [SerializeField, Range(1, 255)] private int _capacity = 10;
        [SerializeField, Range(1, 255)] private int _debugCapacity = 255;

        [Header("Debug logging")]
        [SerializeField] private bool _debugLogToFile;
        [SerializeField, Tooltip("Path will be Assets/[PathField]/[FileNameField]")]
        private string _debugLogFilePath = "Logs";
        [SerializeField, Tooltip("Path will be Assets/[PathField]/[FileNameField]")]
        private string _debugLogFileName = "debug_log.txt";

        [Header("History logging")]
        [SerializeField] private bool _historyLogToFile;
        [SerializeField, Tooltip("Path will be Assets/[PathField]/[FileNameField]")]
        private string _historyLogFilePath = "Logs";
        [SerializeField, Tooltip("Path will be Assets/[PathField]/[FileNameField]")]
        private string _historyLogFileName = "history_log.txt";

        [Header("Commands")]
        [SerializeField] private ConsoleCommand[] _commands;
        [SerializeField, Tooltip("Automatically adds all existing console commands in the project.")]
        private bool _addCommandsAutomatically;

        [Header("Mode")]
        [Tooltip("Console will show Debug logged content instead of command history.")]
        public bool DebuggingMode;
        [SerializeField, Tooltip("Verifies command validity in console.")]
        private bool _feedback;

        // Console UI elements
        [SerializeField] private TMP_InputField _field;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private UnityEngine.UI.Scrollbar _scroll;

        public static Console Instance;
        
        /// <summary>
        /// Get system time in format [hh:mm:ss]
        /// </summary>
        private string Time => $"[{DateTime.Now.ToString("HH:mm:ss")}]";
        /// <summary>
        /// Get system time in format YearMonthDay
        /// </summary>
        private static string Date => DateTime.Now.ToString("yyyyMMdd");

        /// <summary>
        /// Sets singleton
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initializes 
        /// </summary>
        private void Start()
        {
            _field.onSubmit.AddListener(OnSubmit);
            if (_feedback)
            {
                Trigger += OnFeedBack;
                Vec2 += OnFeedBack;
                Vec3 += OnFeedBack;
                Vec4 += OnFeedBack;
                Float += OnFeedBack;
                Int += OnFeedBack;
            }
            Vec4 += SetColor;
            Log("Started logging");
        }

        /// <summary>
        /// Unsubscribes from Actions
        /// </summary>
        private void OnDisable()
        {
            Vec4 -= SetColor;
            if (_feedback)
            {
                Trigger -= OnFeedBack;
                Vec2 -= OnFeedBack;
                Vec3 -= OnFeedBack;
                Vec4 -= OnFeedBack;
                Float -= OnFeedBack;
                Int -= OnFeedBack;
            }
        }

        /// <summary>
        /// Set color of console background or console text
        /// </summary>
        /// <param name="str"></param>
        /// <param name="color"></param>
        private void SetColor(string[] str, Vector4 color)
        {
            if (str[0] == "color")
            {
                if (str[1] == "bg")
                    _text.transform.parent.parent.parent.GetComponent<UnityEngine.UI.Image>().color = color;
                if (str[1] == "text")
                    _text.color = color;
            }
        }

        /// <summary>
        /// Feedback from console commands
        /// </summary>
        /// <param name="args"></param>
        private void OnFeedBack(string[] args)
        {
            if (args.Length == 1)
                Add($"Command executed ({args[0]}).");
            else
                Add($"Command executed ({args[0]} params: {args[1]}).");
            UpdateConsoleTextField();
        }

        /// <summary>
        /// Feedback from console commands
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args"></param>
        /// <param name="value"></param>
        private void OnFeedBack<T>(string[] args, T value)
        {
            if (args.Length == 1)
                Add($"Command executed ({args[0]}) with args {value.ToString()}.");
            else
                Add($"Command executed ({args[0]} params: {args[1]}) with args {value.ToString()}.");
            UpdateConsoleTextField();
        }
#if UNITY_EDITOR
        /// <summary>
        /// Updates console commands from asset database
        /// </summary>
        private void OnValidate()
        {
            if (_addCommandsAutomatically)
            {
                int count = _commands.Length;
                var assets = AssetDatabase.FindAssets("t:ConsoleCommand");
                _commands = new ConsoleCommand[assets.Length];
                for (int i = 0; i < assets.Length; i++)
                {
                    _commands[i] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assets[i]), typeof(ConsoleCommand)) as ConsoleCommand;
                }
                string str = "";
                if (count > _commands.Length)
                    str = $"{count - _commands.Length} commands were removed.";
                else if (count < _commands.Length)
                    str = $"{_commands.Length - count} commands were added.";
                Debug.Log($"Refreshed console commands... {str}");
            }
        }
#endif

        /// <summary>
        /// Log to debug console
        /// </summary>
        /// <param name="str">string to log</param>
        public static void Log(string str)
        {
            Instance._debug.Enqueue(str);
            if (Instance._debug.Count > Instance._debugCapacity)
                Instance._debug.Dequeue();
            if (Instance.DebuggingMode)
            {
                Instance.UpdateConsoleTextField();
            }
            if(Instance._debugLogToFile)
            {
#if UNITY_EDITOR
                
                string dirPath = $"{Application.dataPath}/{Instance._debugLogFilePath}";
#else
                string dirPath = $"{Application.persistentDataPath}/{Instance._debugLogFileName}";
#endif
                string fullPath = $"{dirPath}/{Date}_{Instance._debugLogFileName}";
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                if (!File.Exists(fullPath))
                {
                    FileStream fs = File.Create(fullPath);
                    fs.Dispose();
                }
                StreamWriter sw = File.AppendText(fullPath);
                sw.Write($"{Instance.Time} {str}\n");
                sw.Close();
            }
        }

        /// <summary>
        /// Clears the console and history
        /// If in debugging mode, clears console and debug history
        /// </summary>
        public static void Clear()
        {
            if (Instance.DebuggingMode)
                Instance._debug.Clear();
            else
                Instance._history.Clear();
            Instance._text.text = "";
        }
        
        /// <summary>
        /// Add string to console history
        /// </summary>
        /// <param name="str">string to add</param>
        private void Add(string str)
        {
            _history.Enqueue($"{Time} {str}");
            if (_history.Count > _capacity)
            {
                _history.Dequeue();
            }
            if (Instance._historyLogToFile)
            {
#if UNITY_EDITOR
                string dirPath = $"{Application.dataPath}/{Instance._historyLogFilePath}";
#else
                string dirPath = $"{Application.persistentDataPath}/{Instance._historyLogFilePath}";
#endif
                string fullPath = $"{dirPath}/{Date}_{Instance._historyLogFileName}";
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                if (!File.Exists(fullPath))
                {
                    FileStream fs = File.Create(fullPath);
                    fs.Dispose();
                }
                StreamWriter sw = File.AppendText(fullPath);
                sw.Write($"{Instance.Time} {str}\n");
                sw.Close();
            }
        }

        /// <summary>
        /// Submits text from textfield
        /// </summary>
        public void Submit()
        {
            OnSubmit(_field.text);
        }

        /// <summary>
        /// Builds history into a single string
        /// </summary>
        /// <returns> string of console history or string of console debug history </returns>
        private string BuildConsoleString()
        {
            string[] history = DebuggingMode ? _debug.ToArray() : _history.ToArray();
            string consoleString = "";
            for (int i = 0; i < history.Length; i++)
            {
                consoleString += $"{history[i]}\n";
            }
            return consoleString;
        }

        /// <summary>
        /// Update the console view
        /// </summary>
        private void UpdateConsoleTextField()
        {
            _text.text = BuildConsoleString();
            _scroll.value = 0;              
        }

        /// <summary>
        /// Builds a Commands list with descriptions
        /// Ignores Commands with Help disabled
        /// </summary>
        /// <returns> Command list </returns>
        private string BuildHelpString()
        {
            if(_commands == null || _commands.Length < 1)
            {
                return "I need some too (no existings commands found....)";
            }
            string helpString = "";
            for(int i = 0; i < _commands.Length; i++)
            {
                if(_commands[i].Help)
                    helpString += $"\n{_commands[i].Keyword} - {_commands[i].Description}";
            }
            return helpString;
        }

        /// <summary>
        /// Parses command string and act accordingly depending on the command
        /// </summary>
        /// <param name="commandString">string containing the command</param>
        public void OnSubmit(string commandString)
        {
            Add(commandString);
            if (_commands == null || _commands.Length < 1)
            {
                Add("Unable to locate any console commands....");
                return;
            }
            _field.text = "";
            int commandIndex = -1;
            string[] command = commandString.ToLower().Split(" ");
            if(command[0] == "clear")
            {
                Clear();
                return;
            }
            void help(string helpString)
            {
                Add(helpString);
                UpdateConsoleTextField();
            }
            if(command[0] == "help")
            {
                if(command.Length == 1)
                {
                    help(BuildHelpString());
                    return;
                }
                for(int i = 0; i < _commands.Length; i++)
                {
                    if(_commands[i].IsValid(command[1]))
                    {
                        if(_commands[i].Help)
                            help($"{_commands[i].Keyword} - {_commands[i].Description}");                    
                        return;
                    }
                }
                help($"Unknown command... Command help will give all available commands. Help [command] will give help for that specific command.");
                return;
            }

            for(int i = 0; i < _commands.Length; i++)
            {
                if(_commands[i].IsValid(command[0]))
                {
                    commandIndex = i;
                    break;
                }
            }
            void invalid()
            {
                Add("Invalid command!");
                UpdateConsoleTextField();
            }
            if (commandIndex == - 1)
            {
                invalid();
                return;
            }

            ConsoleCommand cmd = _commands[commandIndex];
            
            switch (cmd.Command)
            {
                case CommandType.Trigger:
                    if (command.Length > 1)
                        Trigger?.Invoke(new string[] { cmd.Keyword, command[1] });
                    else
                        Trigger?.Invoke(new string[] { cmd.Keyword }); 
                    break;
                case CommandType.Vector:
                    if (command.Length == 4)
                    {
                        if(float.TryParse(command[2], out float x) && 
                            float.TryParse(command[3], out float y))
                        {
                            Vec2?.Invoke(new string[] { cmd.Keyword, command[1] }, new Vector2(x, y));
                            break;
                        }
                    } else if (command.Length == 5)
                    {
                        if (float.TryParse(command[2], out float x) && 
                            float.TryParse(command[3], out float y) && 
                            float.TryParse(command[4], out float z))
                        {
                            Vec3?.Invoke(new string[] { cmd.Keyword, command[1] }, new Vector3(x, y, z));
                            break;
                        }
                    } else if (command.Length == 6)
                    {
                        if (float.TryParse(command[2], out float x) && 
                            float.TryParse(command[3], out float y) && 
                            float.TryParse(command[4], out float z) &&
                            float.TryParse(command[5], out float w))
                        {
                            Vec4?.Invoke(new string[] { cmd.Keyword, command[1] }, new Vector4(x, y, z, w));
                            break;
                        }        
                    }
                    invalid();
                    break;
                case CommandType.Float:
                    float floatValue;
                    if (float.TryParse(command[2], out floatValue))
                    {
                        Float?.Invoke(new string[] { cmd.Keyword, command[1] }, floatValue);                        
                    } else if(float.TryParse(command[1], out floatValue))
                    {
                        Float?.Invoke(new string[] { cmd.Keyword }, floatValue);
                    }
                    invalid();
                    break;
                case CommandType.Int:
                    int intValue;
                    if (int.TryParse(command[2], out intValue))
                    {
                        Int?.Invoke(new string[] { cmd.Keyword, command[1] }, intValue);
                    }
                    else if (int.TryParse(command[1], out intValue))
                    {
                        Int?.Invoke(new string[] { cmd.Keyword }, intValue);
                    }
                    invalid();
                    break;
                default:
                    invalid();
                    break;
            }
            UpdateConsoleTextField();
        }
    }
}
