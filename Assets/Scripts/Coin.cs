using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 50;
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = FindPlayer(other);
        if (collected || player == null || !player.CanCollectPickups)
        {
            return;
        }

        ScoreManager manager = FindAnyObjectByType<ScoreManager>();

        if (manager == null || manager.IsScoringStopped)
        {
            return;
        }

        collected = true;
        manager.RegisterCoin(coinValue);
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private static PlayerMovement FindPlayer(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerMovement player))
        {
            return player;
        }

        return other.attachedRigidbody != null
            ? other.attachedRigidbody.GetComponent<PlayerMovement>()
            : null;
    }
}
