using Cysharp.Threading.Tasks;

public interface IUsableItem
{
    /// <summary> 사용 성공시 true 반환 </summary>
    UniTask<bool> Use();
}
