namespace Anon.Os.Gaming;
public sealed class GamingMode
{
 public bool IsActive { get; private set; }
 public event EventHandler<bool>? Changed;
 public void Enable(){if(IsActive)return;IsActive=true;Changed?.Invoke(this,true);}
 public void Disable(){if(!IsActive)return;IsActive=false;Changed?.Invoke(this,false);}
 public IDisposable BeginSession(){Enable();return new Scope(this);}
 private sealed class Scope(GamingMode owner):IDisposable{public void Dispose()=>owner.Disable();}
}
