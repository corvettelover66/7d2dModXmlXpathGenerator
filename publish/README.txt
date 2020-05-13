PrProject Title
7 Days To Die Mod Creator

About Me
Thanks for downloading my application! A little about me. I developed this application while waiting for my first boy to be born, much to the disdain of my lovely wife :)
With the new baby on the way we are struggling to make ends meet. If you like the app,
please don't hesitate to drop us a donation. And know, we really appreciate it!

Good Luck with you mods and please don't forget to leave an endorsement if you like the app!
Also, if you use this to create other mods Leave a link to my mod so other people can find it too.

Useful Tips
The app provides many useful tips to aid the user. 

It is useful to know all menu commands can be executed by following the alt command. To see what is available press and hold alt, then press the key of the letter underlined.
For example to easily save all files press alt-f, alt-s.

Description
An xml object creation app that allows full xml Xpath capabilities. Designed as a mod editor for 7 days to die, however any
xml files should be able to be loaded and added to.
The idea is to generate XPath to the target xml objects rather than editing and writing xml directly.

Comes with many features for easier mod development including:
- Auto Move of XML files to the Game Folder on save
- Instant XML generation view
- Work with multiple object types at the same time and save all of them to the correct files with one click.
- Ability to work on multiple mods using the Custom Tag setting
- Search the existing xml files for objects and easily identify what is needed and what is contained in game
- Auto generation of any Xpath command : Set, Set Attribute, Remove, Append, Insert Before, and Insert After, on all in game objects.
 - This gives users the ability to change, remove, edit, and add new objects to any existing object in game. All without needed to type one line in an xml file.

Getting Started
A typical program flow:
- Start the application
- Set a custom tag
- Load necessary xml files from the Game Config directory. Example: Load the recipes.xml file
- Add completely new objects using the "Add New Object" button in the left window pane.
and/or
- Search the existing objects using the search trees in the right window pane.
- Here you can also target existing objects for object injection. Just click on the object you want to inject into.
- View generated XML instantly in the Center View Panel
- When the generated xml looks good click "Save ALL XMLs"
- If Auto Move is on the files will be instantly copied to the chosen directory.
- If this is the correct Game Folder "Mods" then the mod should be ready to use in game.
- You need to add a "ModInfo.xml" file to GameDirectory/7 Days To Die/Mods/{ModName or CustomTagName}/ with your mod information
contained within that looks something like:
<xml>
	<ModInfo>
		<Name value="MyNewAwesomeMod"/>
		<Description value="A Kick ass mod developed with ease"/>
		<Author value="ThatJonesyGuy"/>
		<Version value="1.0"/>
	</ModInfo>
</xml>

Dependencies
Microsoft .NET 3.5 or higher

Installing
Just download the Archive and extract it to a Folder you want the application to live

Help
Message me on the nexus with full details including steps performed.
The log.txt in the output folder also contains the stack trace of any error for the application.
If you are having issues, send me the end of the log. There will be a timestamp above the error.
Send me the error and it will help me debug your issue.
Please feel free to let me know if there is an issue with a particular file or Xpath command.
The more feedback the better. Your feedback can help make this application better.
It is difficult to discover bugs without replicating them first.
The more descriptive and informative you can be the better.

Authors
@Wrathmaniac or @ThatJonesyGuy

TODO
+ = Important, more pluses means more important
- = Not Important

EASY:
-Add a setting to change the search box threshold

MEDIUM:
-Add a button to open xml files in an external program.
-Change the way settings are displayed potentially so adding settings becomes easier.
-Finish load Mods Directory Menu Item for loading all mods in the game directory. 
-Add a popup on edit, to remove the object from the xml or not.
+Add logic to auto remove config tags from the files.
-When adding a name in the new object view update the button header, essentially use the combo box to update the button name.
+Add the ability to add comments in the direct edit view.
++Add code completion to the direct edit box. http://avalonedit.net/documentation/
++Add a view to add the mod info.xml file.
+++Add the ability to move lines in the direct edit view.
+++Change property tags to use common attributes for the name and not the tag name.

HARD:
-When updating a mod, using the Load mod menu item, merge the files.
-Add the possibility to add multiple attributes to the xpath action.
-Finish the duplicate an object in the left new object panel feature. (This is essentially already in the app. With the copy object command in the search tree you can achieve this functionality)
-Add the ability to validate Recipes against the items xml to help with in game issues.
-In mod trees, on right click, have option to search a file for dependencies.
-Add in functionality to handle the locilization file.
-Find a way to live update search trees on file changes.
-Remove all hardcoded ui strings into a properties file for easily translating the application to other languages.(Not super hard, just time consuming)
+Add side by side search trees for easier comparison.

Version History
1.6.2
 - Added code to reuse context menu in the mod search trees. This was something that was done on the regular search trees to conserve used memory.
 - Re-added the call to the Garbage Collector on removal of ui generated view items. This makes it so that on removal of any object memory should be freed. That means that as you work in the app, memory used should be freed more regularly. Originally I removed this due to recommendations against it. However, I have not noticed a negative impact on performance, only a positive impact. Therefore I'm keeping it.
 - Added the edit object function to game trees. Also changed the function name to "Copy Object" in the UI.
 - Fixed, sound xml did not add all objects correctly.
 - Fixed bug, buttons in new object window were using access keys, potentially blocking the defined access keys. 
 - Added functionality for empty tags, before there was no application way to add these tags. Now there is a checkbox to add an empty tag. Example, recipes.xml wildforgecard category has a checkbox to be added in generated xml.
 - Removed children from onHover in search boxes in the search trees. It was really annoying in the main search box to have the on hover popup blocking the entries.
 - Added a combine feature to direct edit windows. This allows users to combine on top level append tags into the same tag. This is to optimize search trees so app generated appends can be combined into a single tag. 
 - Added an undo all menu button to the DirectEditWindow. This undos any changes in the file since the time of opening the window.
 - Changed splash screen popup to be yes or no.
 - Fixed, when loading a mod, the search tree mod file combo box does not update.
 - Added a new context menu item to new object trees, to flag for ignoring the tree when saving.
 - Added an auto move button to the main UI, this uses the same function as the stage all menu item
 - Added access keys to the main ui for save and stage. To save all xml files users can press alt-s, to stage all mod files users can press alt-a.
 - Reorganized the direct edit ui elements to be next to each other.
 - Added a new combo box in the new object window that fills with common attributes from all mod files or the current slected mod. This includes a CheckBox to decide for each added object. You should note that even objects added from the search trees will use the checkbox. 
 - Added drag and drop option to direct edit view. Users can select text and dag and drop it elsewhere in the doc.
 - Added the show tabs option to the direct edit view.
 - Added current line highlighting to the direct edit view.
 - Added rectangular selection as well. Just press alt when clicking.
 - Added context menu to New Object Trees to hide any unused attributes. This is useful when creating objects that are all similiar and you don't need the attributes. If a node has hidden attributes you will see a (*) in the tree name.

1.6.1
 - Fixed crashing issue when using the edit object function on a Mod Search Tree.
 - Fixed crashing issue when clicking add mod search tree when the selected file is blank.
 - Fixed issue with the edit functionality was displaying to many times. Also modifies the error message to be more descriptive as to the issue.
 - Fixed issue with the Auto Move feature. If the mod had other directories and a localization.txt, these files and directories were ignored, this is now fixed. 
 - Fixed combo boxes from set attribute trees, from the New Object view, did not zoom in and out correctly. 
 - Fixed insert Before and insert after not working properly on attributes in search trees.
 - Added mod name to direct edit window title when opening a mod file in the direct edit window.
 - Added the horizontal scroll bar to the New Object View.
 - Added children to the on hover in the search tree. With this comes a new checkbox to decide for each tree.
 - Added a checkbox to include comments or not in newly generated search trees.
 - Added an option to overwrite local files when loading a mod. This is so you can reload mods. Warning does not MERGE. Any changes to the mod by you will be overritten.
 - Added the Collapse parent right click menu to the mod search trees.
 - Added functionality clicking on an objects, in search trees,  copies the first attribute. Now you can click the main object for the first attribute or open it and click individual attributes.
 - Added the ability to expand and collapse xml nodes in the direct edit view. Includes a context menu to expand or collapse all nodes in the document.
 - Changed all xaml ui elements in main wndow to be center aligned. This is to have better consistancy in the app.
 - All generated attribute values are trimmed. Meaning accidental white spaces in the new object view input boxes are automatically removed.
 - When adding a mod object, the app reloads all wrappers to ensure any xml updates are reflected in the search tree.
 - On saving in direct edit view, xml is validated and warns the user if the xml is invalid. 
 - Removed warning pop-up when saving in the direct-edit view.
 
1.6
 - Fixed a small issue when loading a mod with the boxes in the view not updating correctly.
 - Fixed crashing issue if the user did not set the custom tag with a new install.
 - Fixed font when using xpath commands. Now it should be uniform when adding new objects to the view.
 - Title for CustomTag popup was incorrect when editing a mod name.
 - Added better resolution handling for the CustomTag popup. Also cleaned up the pop up view to fill the space better.
1.5.9
 - Fixed app crashing when selecting a mod for direct edit opening. There was an issue when you click save it updated the mod files combo box incorrectly.
 - Fixed issue with the center game file combo box was not updating when loading a new game file.
 - Adding a setting to turn writing the timestamp on/off when saving xml. Off by default.
 - Added missing tooltips to the new view elements from v1.5.8
1.5.8
 - Made direct edit view resolution friendly.
 - Changed label to be purple in direct edit view.
 - Added appropriate title to the Custom Tag Popups
 - Added better resolution handling to Custom Tag Popup.
 - Added zooming to the New Object view. Now all views can become bigger or smaller.
 - Removed width setting on New Objects Combo boxs. The width will adjust itself based on contents.
 - Cleaned up the zooming to ensure no issues with zooming too small.
 - Added error handling if users select the config directory and not the main mod directory when adding new mods.
 - Removed all width attributes. Now view objects' width is based on text within. 
 - Added the ability to copy objects from mods directly into the new object view.
	- In the mod search tree users can now right click on an object in the tree, then click "edit" and the object is moved into the new object window.
	- This feature allows users to change other mods much easier and add those changes into their own mods or even the existing mod easily.
	- BEWARE: The feature does not Auto remove the original entry so it may need to be removed manually if you want to replace it.
- Moved the mod file selection ui elements from the search tree panel, into the file selection panel.
- Added new UI elements to open game files directly.
- Fixed small issue in the center view. If the xml was invalid for a mod file then the app would crash when selecting that file.  
1.5.7 
 - Changed xaml to use correct Auto and fill values, rather than hardcoded numbers. Tested with multiple resolutions.
1.5.6
 - Collapsing parent was only collapsing the top most tree. Now it collapses just the immediate parent.
 - Reduced search box threshold to 15, now any treeview with 15 or more children will have a search box.
 - Made all combo boxes sorted in abc order. Now it should be much easier to find things.
 - Made removing an item in the new object view more usable. Now users can remove specific tags, rather than the whole object. 
	- Clicking remove on an object that is the last one in the tree will try to remove the top tree. A warning is displayed and users chooses.
 - Memory optimization done right.
	- Added optimization by reducing new ToolTips. Now the app reuses tooltips when they are the same.
	- Added optimazation on the ContextMenu used in the search view. This reduces used memory and load times by 60%!
		After profiling the app I determined the MenuItems in ContextMenus were using a considerable amount of memory.
		The search trees used two seperate ContextMenus that were generated and recreated for every treeview.
		This was terribly inefficent on memory, after making some small edits to reuse a ContextMenu when possible, a search views memory was reduced by 60%, which in turn reduces loading times considerably.
		En example is the items.xml file, version 1.5.5 memory usage was 2.8 gb (Yes, Giga bytes). Version 1.5.6 is 900mb. 
		As my old proffessor use to say don't worry about memory until it is a problem. I did not anticipate a Context menu to use so much.
 - Removed calls to Garbage collector. After significant research the calls were unnecessary. 
	These calls were cleaning up memory faster without a noticable performance impact, however, all of the documentation
	recommends against this. The memory should be cleaned up automatically by c#.
1.5.5
 - Fixed issues in generated XML for Xpath commands. I have tested every xpath command and the generated XML is very accurate, I'm hoping perfect!
 - Fixed a bug with the Help menu item, it was returning invalid file contents when the file didn't exist.
 - Fixed quick commands for Menu items, the File menu was not working with quick commands.
 - Fixed direct edit view Menu. It was off when side by side.
 - Fixed Direct Edit windows close when main app closes.
1.5.4 
 - Fixed bug in ui where the mod files combo box in the center view would not show correcty on startup.
 - Fixed bug with saving the xml, a newline was continually added at the last tag to the xml pushing the last tag further away.
 - Fixed bug where empty files were being generated.
 - Fixed bug where saving files would add content from other files to a Xui file due to the folder nesting. 
 - Fixed bug with any one liner xpath actions, before the inner text and contents where generated with new lines. Now the app generates the xml like the game expects.
 - Fixed issue with the setattribute command. Before it would generate the xml incorrectly and put a closing quote(") in the wrong place.
 - Fixed default Help message
 - Fixed issues in the generated xml, with appends and inserts before and after on attributes were not one liners.
 - Added shortcuts for every Menu in the app. Just press alt and the letter that is underlined is the next key to press. For example alt-f,alt-s will save all files. 
 - Changed the way DirectEditing works. Now direct edit windows can be open without locking the app.
 - This comes with many new features to handle direct edits. 
 - In the direct edit view users can reload the file from disk.
 - The file can also be validated directly in the view for Xml issues.
 - Added a context menu, remove tree, to the search trees.
 - Added in line numbers to the Xml Views. This is a setting on the object.
1.5.3
- Removed the Memory optimization as I was seeing crashes with large xml files. 
1.5.2
- Fixed another bug in xml generation. AppendBefore and appendAfter were missing a closing tag.
- Fixed a small bug after editing/changing a Mod name the search tree combo box was not updated.
- Added the missing tooltips to the new menu items from v1.5.1.
- Added a new context menu item in the search view to collapse the parent tree.
	- I found this to be incredibly useful for large trees.
- Cleaned up the ToolTips.
- Fixed the label display bug in the direct edit view. The label was cut off when opening initially.
1.5.1
- Fixed small bugs in xml generation output.
	- New Objects were not being tabbed like Commands from the search tree.
	- Some commands were not generating the closing tag.
- Fixed crashing issue with bad Mod names when trying to generate a search tree for a mod file.
- Added new menu item to validate the xml.
	- This uses the current selected mod and validates all xml files for the mod.
	- For any invalid xml, the application gives you the exact xml issue. No logs needed.
- Added a new menu item to Edit a tag name.
	- This is to fix any issues with loaded mods without needing to leave the application to change directory names.
	- Changing a mods directory name in the output folder will still modify the name of the Mod when loaded.
- Changed the location of new objects when clicking a button on an existing object form.
	- Now when adding a new object the object will go directly below. This has no effect on xml correctness. This just makes the generated xml cleaner.
- Added a huge optimization to the used memory. For large files the memory used is still high but clearing the views clears the used memory as well.
1.5
- Remove the help feature from the search tree contet menu and add ToolTips to the content menu instead.
- Added a mouse scroll feature to the xml output field. Now users can increase/decrease the font of the output box by just ctrl-scrolling on the mouse wheel.
- Added a mouse scroll feature to the search tree view. Now users can increase/decrease the font of the search trees by just ctrl-scrolling on the mouse wheel.
- Added features to support external mods.
	- In the search view there is a new combo box filled with any loaded mods. When used the selected mod displays another combo box and button that can be used to create search trees for Mod files.
	- There is also a new combo box in the center view for switching between custom tags/mods easily.
		- This is still tied to the custom tag setting. 
		- By switching to the mod using the custom tag setting you can add items directly to that mods files.
			- Warning if the mod is not using custom tags you may need to alter the xml to use custom tags.
		- If you want to edit the mods files directly change the custom tag to the Mod and you can open that mods files.
	-Added a Menu Item for loading a mod directory. This is supposed to be the main mod folder with the name of the mod. Typically the folder that comes out of archives from the nexus.
- Added color coding to the search tree and new object window. This uses similiar coloring as the xml.
- Fixed the center button and combo boxes to be fixed to the panel size. They should shrink and grow with the center view.
- Fixed bug where empty files would be generated on saving.
- Fixed a major issue in the object add view for severly complex xml files. 
	If the user tries to load Xui/Controls, for instance, the program would loop endlessly due to child nodes being the same name as parents.
1.4.1 
- Small Bug Fixes 
	- Fix bug in new object window where the top button was not working.
 	- Fixed problem in search tree with children that are search boxes. The search boxes were not displaying when using the search feature.
- Improved the Search Box Logic to search for relevant children. Before it was unreliable as it was including any nodes with XmlWhitspace comments.
	- this refers to the threshold for determining the search box. (Plans to make this a setting)
- Added a file threshold for the large search views. If the file is over 1000K it uses a considerable amount of ramm when creating the search view.
	- The user is given the option when trying to add multiple.
- Font Size change in the Search View.
- Changed the output xml to close childrenless tags with the "/>" instead of a full tag.
1.4 
- Update to the search tree.
	- Now the search tree has better logic for adding search boxes to the tree.
		- The logic for determining when to add a search box is now based on number of children.
		- The search box is also guarenteed to contain only direct children. 
	- Full Xpath capabilities.
		- Now the application allows users to perform any simple Xpath command(Set, Set Attribute, Remove, Append, Insert Before, and Insert After) simply by right clicking on an object in the search tree.
			- This does mean that the insert function has been changed and the tree buttons removed. Technically, before each button created an append command.
1.3
- Added more reliable Xpath Targeting.
- I did some tests with more xml files and discovered some oddities in the search trees generated.
I fixed the issues and in the process created a better Xpath generation for item targeting.
Now the Xpath generated is guaranteed to target the specific item selected.
- Added xml text coloring to the output text box. Thanks to an excellent Nuget package called AvalonEdit!
- Added an OnFocused event to the XmlOutput box to update the output when it is focused.
- This is to make viewing the xml output easier
- Try to fix the resolution issue with the menu.
- I added in some margin spacing to the menu in hopes this fixes the resolution issue.

1.2.1
- Added better handling of issues. Any issues will now be written to the log before the application shuts-down.
This is to aid the end-user determine issues or they can just send me the log and I can better determine the issue.

1.2
- Changed the publish parameters to make the application fully self-contained and into a single file.
- Full code base refector and cleanup.
- Fixed the log output time.
- Now displays the correct date and time.
- Added in the direct editor view and actions
- With this functionality the user can make direct XML edits to the file without leaving the application
- Added an Application Menu
- File Menu
- Load (Moved from center view to menu)
- Save (Extra save button, does same thing as center view save all)
- Move (Performs the auto move function once on command)
- Essentially allows the user to stage the generated mod files to the game directory chosen
- Settings Menu
- Auto Move (Activate or Deactivate)
- A feature to make modding easier.
With one click you can save your generated xml to the appropriate files and the application will automatically move those files to a directory of your choice.
Very useful if set to the actual Game "Mods" Folder. You can create the mod click save and it's almost ready to go.
- Game Mod Directory
- Completely tied to the move functionality and necessary for that feature to work.
This is the directory that the files can be "Auto Moved" to every time you click "Save"
or the directory files will be moved to if the File Menu "Move" is clicked.
- Custom Tag
- View All Settings
- Help Menu
- Displays the README.txt or another message if the README.txt is missing.

1.1.1
- Fixed a crashing issue with the Progressions Target Object Tree
- Fixed the issue with the Vertical Bar not being visible in the XML output view
- Increased the XML Ouput view Font Size
- Added a Right Click Menu to allow the removal of single items in the New Object view.
This is to introduce a better flow to mod creation.
- Fixed log file issue.

1.1
- Fixed an issue with the xml output view not displaying correctly with objects of multiple top tags (such as progressions)
- Added a message to the xml output view to help users understand how to make changes.
- Put the project on a repo.
- Added a search box to the TreeView panel. Now you can filter the TreeView for specific items in the game xml files.
- Also added the ability to add objects to existing objects within the game files.
The application generates the appropriate Xpath expression.
- Created a system for adding to existing xml.
- Cleaned up the output xml to have tabs, correctly formatted in the file and output view.

1.0.1
Small fixes an changes
- Changed the xml viewing page to be editable, however any changes made there are not persisted.
- Fixed issue with crashing if trying to load an xml file with the same name.
- Fixed a Bug in the xml output field where all loaded xml wrappers in the object view did not display saved data.
- The display did not update correctly with all saved xml objects.

1.0
Initial Release
Includes functionality for:
- Creation of new Xml Objects within an xml object using Xpath
- The create page boxes are filled with used values in the loaded xml file
- Viewing generated xml instantly on changing the Data in the Object View
- A tree view to see all objects within the loaded file eaiser
- A quick copy action on the tree view attributes (Click once)
- Note: The application is designed around modding 7 Days to Die,
however, theoretically should work on ANY xml file.

Future Releases:
- Pull all UI strings out into some other file, maybe a constants.
- Better yet an xml file for application translation capabilities.

Acknowledgments
The Fun Pimps for an amazing Game! Without them I would not have created this mod creator.
My company for the .NET licence to develop the app in C#.

awesome-readme
ThatJonseyGuy