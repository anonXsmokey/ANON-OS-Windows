namespace Anon.Os.ControlCenter;
public sealed record ControlCenterState(string Profile="GAMING",bool GamingMode=false,bool AiEnabled=false);
public sealed class ControlCenterModel
{
 public ControlCenterState State{get;private set;}=new();
 public event EventHandler<ControlCenterState>? Changed;
 public void SetProfile(string profile){State=State with{Profile=profile.ToUpperInvariant()};Changed?.Invoke(this,State);}
 public void SetGamingMode(bool enabled){State=State with{GamingMode=enabled};Changed?.Invoke(this,State);}
 public void SetAiEnabled(bool enabled){State=State with{AiEnabled=enabled};Changed?.Invoke(this,State);}
}
