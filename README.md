# Jutils.Console
Simple console implementation based on [ScriptableObjects](https://docs.unity3d.com/Manual/class-ScriptableObject.html) for Unity. ConsoleCommands are ScriptableObjects that can be created from Unity's [Project View](https://docs.unity3d.com/Manual/ProjectView.html). Can function as a runtime debug console and as a runtime command console. Current implementation requires [TextMeshPro](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html).

Disclaimer: This is meant to be a basis for console functionality to expand upon. Idea was to make easily expandable console for my own use. But also decided to share it as an example. Might contain bugs (limited testing) and does not catch possible errors.

# Usage
Create an empty GameObject in scene hiearchy and add Console script to it. Also requires adding TMP_InputField, TMP_Text and Scrollbar in inspector to relevant fields (Field, Text, Scroll). Create new commands by right clicking in project view and navigating Create > Console Command. 

## Command structure
### Trigger
Trigger type will invoke Console.Trigger action (keyword myparam) or (keyword).
### Vector
Vector type will invoke Console.Vec2 action (keyword myparam float float), Console.Vec3 action (keyword myparam float float float) or Console.Vec4 action (keyword myparam float float float float).
### Float
Float type will invoke Console.Float action (keyword myparam float)
### Int
Int type will invoke Console.Int action (keyword myparam int).

## ConsoleCommands Inspector
### Keywords (string[])
Keywords associated with the ConsoleCommand. Allows for easy use of abbreviations when necessary.
### Command (CommandType)
Type of the command.
### Description (string)
Description of the command that is printed into console with help command.
### IncludeInHelp (bool)
Set to false to hide the command in help commands.

## Built-in commands
### clear
Clears the console view and command history or debug history. Depending on the current mode.
### help
Lists all console commands that have IncludeInHelp boolean enabled. Boolean is there to have separate hidden commands if necessary. Format: ConsoleCommand.Keyword[0] - ConsoleCommand.Description. Help command prints out single command description.

## Console Inspector
Custom editor script for the inspector will make it unavailable in runtime as it is not designed to work in runtime.
### Capacity (int)
Capacity for the amount of commands stored in console's history.
### Debug Capacity (int)
Capacity for the amount of debug log data stored in console's history. (Separate Queue from history)
### DebugLogToFile (bool)
Determines if Console should also log to file.
### DebugLogFilePath (string)
File path debug data should be stored at. This path will be at [Application.dataPath](https://docs.unity3d.com/ScriptReference/Application-dataPath.html) if used inside Unity editor and [Application.persistentDataPath](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html) if used in build runtime.
### DebugLogFileName (string)
Name for the debug data file. Note: Date will be added automatically to the filename in format: yyyyMMdd_FileName.
### HistoryLogToFile (bool)
Determines if Commands entered should also log to file.
### HistoryLogFilePath (string)
Same as DebugLogFilePath but for command history.
### HistoryLogFileName (string)
Same as DebugLogFileName but for command history.
### Commands (ConsoleCommand[])
Array of commands the console should include.
### AddCommandsAutomatically (bool)
Uses [AssetDataBase.FindAssets](https://docs.unity3d.com/ScriptReference/AssetDatabase.FindAssets.html) in  [OnValidate()](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnValidate.html) to refresh Commands array.
### DebuggingMode (bool)
Show debug data on console (true). Show command data on console (false)
### Feedback (bool)
Adds feedback to existing Console script Actions that prints them out in the console as received commands.
### Field (TMPro.TMP_TextField)
Input field for the commands.
### Text (TMPro.TMP_Text)
Console contents.
### Scroll (UnityEngine.UI.Scrollbar)
Used to update scrollbar's scroll value when console content is updated.

## Console Class
### Trigger (Action<string[]>
Trigger actions, usage Console.Trigger += MyMethod.
### Vec2 (Action<string[], Vector2)
Vector2 actions, usage Console.Vec2 += MyMethod.
### Vec3 (Action<string[], Vector3)
Vector3 actions, usage Console.Vec3 += MyMethod.
### Vec4 (Action<string[], Vector4)
Vector4 actions, usage Console.Vec4 += MyMethod.
### Float (Action<string[], float)
Float actions, usage Console.Float += MyMethod.
### Int (Action<string[], int)
Int actions, usage Console.Int += MyMethod.
### Instance (Console)
Singleton for the console.
### Log (string)
Log debug data, usage Console.Log(myString). If debugging mode is disabled by debug to file is enabled will write data to file.
### Clear (void)
Clears console history or Debug history if in DebuggingMode. Note: when logging to file the files remain intact. Built-in command clear calls this function.
### Submit (void)
Gets text written in text field and calls OnSubmit(_textField.text).
### OnSubmit (string)
Parses console commands and triggers relevant Actions.

## Adding new types of commands
- Add new value into CommandType enum
- Add Action for the command type in Console class.
- Add case for the command type in Console.OnSubmit method


# Example
```
using Jutils.Console;
public class MyExampleClass : MonoBehaviour
{
	void Awake()
	{
		Console.Trigger += MyTriggerMethod;
		Console.Vec2 += MyVector2Method;
	}
	void MyTriggerMethod(string[] args)
	{
		if (args[0] == "mykeyword" && args.Length == 1)
		{
			// Do Something
		} 
		else if(args[0] == "mykeyword")
		{
			if (args[1] == "myparam")
			{
				// Do something else
			}
		}
	}
	void MyVector2Method(string[] args, Vector2 myVector)
	{
		if(args[0] == "mykeyword")
		{
			if (args[1] == "myparam")
			{
				// Do Something with myVector
			}
			if (args[1] == "myparam2")
			{
				// Do something else with myVector
			}
		}
	}
}
```
