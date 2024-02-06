using Flamui;

namespace Sample.ComponentGallery;

public partial class RootComponent : FlamuiComponent
{
    [Parameter(isRef:true)]
    public required string Input { get; set; }

    [Parameter]
    public bool ShouldShow { get; set; }

    public override void Build()
    {
        Create("").ShouldShow(true).Build();
    }
}
