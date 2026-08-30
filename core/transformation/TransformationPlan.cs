namespace Anon.Os.Core.Transformation;
public sealed record TransformationAction(string Id,string Description,bool Reversible);
public sealed record TransformationPlan(string Profile,IReadOnlyList<TransformationAction> Actions);
public static class TransformationPlanner
{
 public static TransformationPlan Create(string profile)=>profile.ToUpperInvariant() switch
 {
  "LITE"=>new("LITE",new[]{new TransformationAction("baseline","Apply conservative performance policy",true)}),
  "GAMING"=>new("GAMING",new[]{new TransformationAction("gaming-policy","Apply gaming-focused safe policy",true)}),
  "ULTRA"=>new("ULTRA",new[]{new TransformationAction("high-performance-policy","Apply high-performance safe policy",true)}),
  _=>throw new ArgumentException("Unknown ANON profile.",nameof(profile))
 };
}
