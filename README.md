The TFS Productivity Tools project was designed to provide TFS administrators with helper extensions for Source Control or Work Item Tracking tasks.

For running these extensions the installation of Visual Studio Professional is required.
The extensions tested with Visual Studio 2012.

|Tool Description|Runs from Visual Studio Window|
|----------------|------------------------------|
|Modify Work Item link types. |Team Explorer| 
|Completely destroy Work Items. |Query Results| 
|Export Work Items to Microsoft Word document. |Team Explorer| 
|Provide workaround for several merge features not implemented by TFS: <br/>1.TFS merge leads to bulk check-in operation that puts files from all previous changesets into one big merge changeset. <br/>2.TFS allows only for consecutive changesets being cherry-peeked by merge operation. <br/>3.TFS doesn’t allow choosing changesets for cherry-peek merge by selecting work items. <br/>4.TFS merge dialog doesn’t have “force” and “baseless” options. |Source Control Explorer, Query Results|
|Emulate commandline task and write outputs to Output window: <br/> tf.exe get /recursive /preview "itemspec" |Source Control Explorer|
|1. Update modification time for checked-out files to their latest check-in time.<br/>2. Directly modify changeset's check-in time in TFS database. |Source Control Explorer, History|

### Installation

For installing anyone of the extensions double-click on VSIX file and follow the installer instructions.

image001

### ChangeLinkTypes

This extension modifies link types between the Work Items returned from Work Item query.<br/>
To execute it select any query under Work Items node in Team Explorer window and click on "Change Query Link Types"

image002

After that, in next dialog choose original and new wanted link types.

image003

### DestroyWorkItems

This extension completely destroys (not closes) Work Items returned from Work Item query.<br/>
To execute it select a number of Work Items in Query Results window and click on  "Destroy Work Items"

image004

### Export2Word

This extension exports Work Items returned from Work Item query to Microsoft Word document in paragraph style. It uses MS Office Interop package and requires a local installation of Word.<br/>
To execute it select any query under Work Items node in Team Explorer window and click on "Export to Microsoft Word"

image005

### ExtendedMerge

Extended Merge extension provides workaround for several merge features not implemented by TFS:
1.TFS merge leads to bulk check-in operation that puts files from all previous changesets into one big merge changeset. 
2.TFS allows only for consecutive changesets being cherry-peeked by merge operation. 
3.TFS doesn’t allow choosing changesets for cherry-peek merge by selecting work items. 
4.TFS merge dialog doesn’t have “force” and “baseless” options. 

#### Initializing ExtendedMerge extension

The list of merge candidates can be obtained in two ways:
1.From Source Control Explorer window.
In this case all history changesets for a specific server path are parsed.
image006 
2.From Query Results window.
In this case all changesets, linked to the selected Work Items are parsed.
image007 

After initialization stage extension opens Visual Studio tool window pane with a grid that shows useful information about parsed changesets:
* Checked/unchecked status. 
* Work item ID. 
* Changeset ID. 
* Changeset check-in date 
* Changeset creator. 
* The change types this changeset contains. 
* Possible merge options 
--* None – merge is impossible (don’t mess with “discard” commandline option) 
--* Baseless – baseless merge 
--* Force – force merge 
--* Candidate – regular merge 
* Merge source path (can be modified). 
* Changeset comment field. 

image008

The Merge Target Location field contains the path to a folder or a branch in source control.

Merge types are calculated based on shared Merge Target Location path and an individual changeset Source Path.

#### Running ExtendedMerge extension

Extension runs all actions from toolbar buttons (some of them are duplicated on grid context menu also).

image009

 
|Menu item name|Menu item description|
|--------------|---------------------|
|Link to Work Items |When checking in merge results link new changesets to work items the same way as they were linked in original changesets. |
|Normal Merge |Do regular merge based on merge candidates. |
|Conservative Merge |Use “Conservative” merge option. It produces more merge conflicts. |
|Refresh |Refill changeset merge types. |
|Merge |Do merge. |
|Resolve |Show merge conflicts. |
|Changeset details |Show changeset details. |
|Work Item details |Show Work Item details. |
|Navigate to Server Path |Navigate to server path in Source Control Explorer. |
|Edit Server Path |Edit server paths (one or multiple). |
|Copy to Clipboard |Copy selected changeset details to clipboard. |
|Mark All Items |Check all grid items. |
|Unmark All Items |Uncheck all grid items. |

 
#### Resolving merge conflicts

During merge operation merge conflicts can occur.
In this case click on OK button and resolve conflicts with standard TFS Resolve Conflicts dialog.

image010

image011

### GetPreview

This extension emulates TFS commandline operation: tf.exe get /recursive /preview "itemspec"
To execute it select a path in Source Control Explorer window and click on  "Emulate Get-Preview".

The results are print to Output window.

image012

### ModifyCheckinDate

This extension consists from 2 parts:
1.Updates modification time for checked-out files to their latest check-in time.
 It is accessible from Source Control window & History window. 
2.Directly modifies changeset's check-in time in TFS database.
 It actually runs SQL statement: UPDATE tbl_Changeset SET CreationDate=’?’ WHERE ChangeSetId=‘?’
It is accessible from History window. 

image013

image014

### Release Notes
The ExtendedMerge uses a number of internal Microsoft classes. It can possibly lead to some tool malfunction in the next TFS versions: 

|Feature |Classes|
|--------|-------|
|Show browse server folder dialog |Microsoft.TeamFoundation.Build.Controls.VersionControlHelper.ShowServerFolderBrowser| 
|Show merge progress dialog |Microsoft.TeamFoundation.VersionControl.Controls.ProgressMerge |
|Show resolve conflicts dialog |Microsoft.TeamFoundation.Client.Arguments<br/>Microsoft.TeamFoundation.VersionControl.CommandLine.CommandResolve |

Extension packages used icons from GNOME project:  http://art.gnome.org/themes/icon 
