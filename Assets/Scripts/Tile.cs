using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Tile : MonoBehaviour
{
    public TileState State { get; private set; }
    public TileCell Cell { get; private set; }
    public bool Locked { get; set; }

    private Image Background;
    private TextMeshProUGUI Text;

    private void Awake()
    {
        Background = GetComponent<Image>();
        Text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetState(TileState State)
    {
        this.State = State;

        Background.color = State.BackgroundColor;
        Text.color = State.TextColor;
        Text.text = State.Number.ToString();

    }

    public void Spawn(TileCell Cell)
    {
        if (this.Cell != null)
        {
            this.Cell.Tile = null;
        }

        this.Cell = Cell;
        this.Cell.Tile = this;

        transform.position = Cell.transform.position;
    }

    public void MoveTo(TileCell Cell, System.Action onComplete = null)
    {
        if (this.Cell != null)
        {
            this.Cell.Tile = null;
        }

        this.Cell = Cell;
        this.Cell.Tile = this;

        StartCoroutine(Animate(Cell.transform.position, false, onComplete));

    }

    public void Merge(TileCell Cell, System.Action onComplete = null)
    {
        if (this.Cell != null)
        {
            this.Cell.Tile = null;
        }

        this.Cell = null;
        Cell.Tile.Locked = true;

        StartCoroutine(Animate(Cell.transform.position, true, onComplete));

    }

    private IEnumerator Animate(Vector3 to, bool merging, System.Action onComplete)
    {
        float elapsed = 0f;
        float duration = 0.15f;

        Vector3 from = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = to;

        if (merging)
        {
            Destroy(gameObject);
        }

        onComplete?.Invoke();
    }


}
