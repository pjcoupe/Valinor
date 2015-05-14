using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;

/// <summary>
/// Provides a Unity Editor function that 'fixes' the animation length on selected AnimationClip assets.
/// You can also select directories to process the animations inside.
///
/// It adds the menu entry "Fix Animation Length" to the Assets menu. You can also access it when right
/// clicking on the Project view. Hence, you can select assets in the Project view and right click -> fix.
///
/// The assets need to be saved in order for this to work. Thus, the script saves all assets before commencing.
///
/// Based on alstonc99's script at http://forum.unity3d.com/threads/131340-Reduce-animationclip-length.
/// Thanks for finding the solution and sharing it.
///
/// This script is in the public domain, feel free to improve and share.
///
/// NOTE: Unity seems to cache the selected assets. If you select all your assets and run this script, Unity
///       might hang for a few seconds. Afterwards processing all your assets again is faster.
///      
/// DISCLAIMER: We currently don't know what purpose the bogus entry serves. Use at your own risk!
/// </summary>

public static class FixAnimationLength
{
	private const SelectionMode selectionMode   = SelectionMode.Assets |
		SelectionMode.DeepAssets |
			SelectionMode.TopLevel |
			SelectionMode.ExcludePrefab;
	
	private const string        tokenToReplace  = "m_LocalEulerAnglesHint";
	private const string        replacement     = "REMOVE_ME_LocalEulerAnglesHint";
	
	
	[MenuItem( "Assets/Fix Animation Length" )]
	public static void RemoveLocalEulerAnglesHints()
	{
		Debug.Log( "Marking the '" + tokenToReplace + "' entries in the selected AnimationClips with '" + replacement + "'.\n" +
		          "Open the animations in the animation editor of Unity to provide the 'Clean Up Leftover curves' button. " +
		          "Click on the button and confirm the dialog to finally fix the animation." );
		
		AssetDatabase.SaveAssets();
		
		string s = "";
		
		foreach ( AnimationClip animationClip in Selection.GetFiltered( typeof( AnimationClip ), selectionMode ) )
		{
			var path = AssetDatabase.GetAssetPath( animationClip );
			
			var changed = Fix( path );
			
			if ( changed )
			{
				s += "Processed\t'" + path + "'\n";
			}
			else
			{
				s += "Skipped\t\t'" + path + "'\n";
			}
		}
		
		AssetDatabase.Refresh();
		
		Debug.Log( s );
		Debug.Log( "Done." );
	}
	
	private static bool Fix( string path )
	{
		List<string> lines = new List<string>();
		
		bool changed = false;
		
		using ( var reader = new StreamReader( path ) )
		{
			var line = reader.ReadLine();
			
			while ( !String.IsNullOrEmpty( line ) )
			{
				changed = changed || line.Contains( tokenToReplace );
				
				// Replace the entry if it is in this line
				line = line.Replace( tokenToReplace, replacement );
				
				lines.Add( line );
				
				line = reader.ReadLine();
			}
		}
		
		// Only write the file if it actually changed
		if ( changed )
		{
			using ( var writer = new StreamWriter( path ) )
			{
				foreach ( var line in lines )
				{
					writer.WriteLine( line );
				}
			}
		}
		
		return changed;
	}
}


