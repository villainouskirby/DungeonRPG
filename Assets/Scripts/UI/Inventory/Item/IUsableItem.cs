using Cysharp.Threading.Tasks;

public interface IUsableItem
{
    UniTask<bool> Use();
}
