MarkEmbling.Forms.Controls
=======================

Just a few custom controls for Windows Forms that I've used in a couple of places.

This package is [available on NuGet](https://www.nuget.org/packages/MarkEmbling.Forms.Controls/) and currently targets .NET 4.7.2 and .NET 5.0.

This is effectively a continuation of the old `MarkEmbling.Utils.Forms` but with a slighty better name.

### Controls

 - `ClipboardAwareTextBox`  
   Inherits from `TextBox` and exposes events for clipboard events (cut/copy/paste).
 - `NativeStyleTreeView`  
 	Extends `TreeView` and adds a property to toggle between the standard .NET TreeView appearance and the 'native' Explorer style appearance (updated expend/contract buttons and selection style)
 - `DragDropTreeView`  
 	Extends the above `NativeStyleTreeView` control and adds the ability to re-order nodes via drag and drop. Remember to change the `AllowDrop` property to true to enable this.
 - `Gauge`  
    Gauge control based upon [AGauge](http://www.codeproject.com/Articles/448721/AGauge-WinForms-Gauge-Control). Currently buggy when using 3D-style needles and there are more features to be implemented, but is usable.

### Examples

A little Windows Forms app which demos some of the controls can be found in `MarkEmbling.Forms.Controls.ExamplesNet472`. 
It's not very comprehensive and targets .NET Framework 4.7.2 (you probably guessed that already).
