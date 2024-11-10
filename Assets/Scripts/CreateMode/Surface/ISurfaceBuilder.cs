using Cysharp.Threading.Tasks;

namespace CreateMode.Surface{
public interface ISurfaceBuilder {
    public UniTask<VertSurfaceRenderer> RequestVerticalSurfaceFromUserAsync(int pointLimit = 2);
}}
