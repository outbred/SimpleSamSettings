# SimpleSamSettings
Simple, cross-platform settings framework based on the idea of many distinct settings classes are better than one huge one. Persistence is configurable, as is the location.

Every application requires user-, application-, and other defined settings.  The maintenance of these settings can be horrendous over time if not property architectured and maintained.

SimpleSamSettings aims to ease the burden of creating, managing, saving, and restoring settings for any application in .NET Standard.

Available on nuget [here](https://www.nuget.org/packages/SimpleSamSettings.Framework/1.0.0)     

## Demo app

  * See the SimpleSamSettings.Demo WPF application
  
## Quick Start Example
```c#
public class MySettings : OverwriteSettingsProfile<MySettings> 
{
   public string SomeProp
   {
      get => Get<string>();
      set => Set(value);
   }
}

Globals.SettingsFileBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyAppName");
```

That's it!  Your settings will be persisted to %appdata%/MyAppName/MySettings.settings in JSON format.  Prefer binary?  No problem!  Put the [Serializable] tag on it and it will auto-serialize as binary.

By default, each change updates the file on disk.  To turn off this behavior, simply set AutoSave to false, like this:
```C#
var settings = new MySettings() { AutoSave = false };
```

...then at any time, you can save it (say, on application exit):
```C#
settings.SaveInstance() OR MySettings.Save(); // OverwriteSettingsProfile<> ensures a singleton instance, so it is a static helper method
```

### Undoing settings changes - the easy way!

Say you have a dialog open for the user to change settings.  The settings changes are live, ideally, but if the user clicks 'Cancel' everything they've done during the lifetime of that dialog should be undoe.  Easy peasy!

Simply set the 'MakeUndoable' flag in the base to ```C#true```.
```C#
settings.MakeUndoable = true;
```
... do your stuff in the dialog ...

To cancel or undo everything:
```C#
settings.RevertChanges();
```
Then set MakeUndoable back to false so it doesn't save off changes:
```C#
settings.MakeUndoable = false;
```

# Usage Advice

Encapsulate your settings into small classes - many, properly encapsulated classes is preferred over one or a few very large ones!  This makes it easier to maintain, easier to use and reuse, and speeds up disk i/o.

### NOTE:
If a collection or object inside the settings class changes, the file will not be autosaved.  It is _recommended_ that the settings class listen for changes on those objects and auto-saves them, as appropriate, but this is in no way enforced and left entirely up to the consumer (you).  Happy settings coding!

### Show me some :heart: and star this repo to support my project

### Created & Maintained By

[Brent Bulla](https://github.com/outbred) ([@outbred](https://www.twitter.com/outbred))

> If you found this project helpful or you learned something from the source code and want to thank me, consider buying me a cup of  <img src="https://vignette.wikia.nocookie.net/logopedia/images/a/ad/Dr._Pepper_1958.jpg/revision/latest?cb=20100924201743" height="25em" />  -  [PayPal](https://paypal.me/brentbulla/)

#### Side note:
Even though this repo is new, this architecture behind this framework has been in production and used by me for several years now on several different projects.  I've re-written it for open source consumption, but it is not something that I've just now come up with - I've refined it and simplified it over the years into something that (now) I think is worthy of sharing with the world.
