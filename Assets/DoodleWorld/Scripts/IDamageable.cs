using Cysharp.Threading.Tasks;

public interface IDamageable
{
    UniTask TakeDamage(int damage);
}