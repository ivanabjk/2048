using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class TileBoard : MonoBehaviour
{
    public GameManager GameManager;
    public Tile TilePrefab;
    public TileState[] TileStates;
    private TileGrid Grid;
    private List<Tile> Tiles;

    private Controls controls;
    private Vector2 moveInput;

    private bool waiting;

    private void Awake()
    {
        Grid = GetComponentInChildren<TileGrid>();
        Tiles = new List<Tile>(16);

        controls = new Controls();

    }
    private void OnEnable()
    {
        controls.Enable();
        controls.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnDisable()
    {
        controls.Gameplay.Move.performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled -= ctx => moveInput = Vector2.zero;
        controls.Disable();
    }

    public void ClearBoard()
    {
        foreach (var Cell in Grid.Cells)
        {
            Cell.Tile = null;
        }
        foreach (var Tile in Tiles)
        {
            Destroy(Tile.gameObject);
        }
        Tiles.Clear();
    }


    public void CreateTile()
    {
        Tile tile = Instantiate(TilePrefab, Grid.transform);
        tile.SetState(TileStates[0]);
        tile.Spawn(Grid.GetRandomEmptyCell());
        Tiles.Add(tile);
    }

    private void Update()
    {
        if (!waiting)
        {
            if (moveInput == Vector2.up)
            {
                MoveTiles(Vector2Int.up, 0, 1, 1, 1);
            }
            else if (moveInput == Vector2.down)
            {
                MoveTiles(Vector2Int.down, 0, 1, Grid.Height - 2, -1);
            }
            else if (moveInput == Vector2.left)
            {
                MoveTiles(Vector2Int.left, 1, 1, 0, 1);
            }
            else if (moveInput == Vector2.right)
            {
                MoveTiles(Vector2Int.right, Grid.Width - 2, -1, 0, 1);
            }
        }
    }


    // start - 0 or the last index; increment - +1 or -1 
    private void MoveTiles(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        bool changed = false;
        for (int x = startX; x >= 0 && x < Grid.Width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < Grid.Height; y += incrementY)
            {
                TileCell cell = Grid.GetCell(x, y);

                if (cell.Occupied)
                {
                    changed |= MoveTile(cell.Tile, direction);
                }
            }
        }

        if (changed)
        {
            StartCoroutine(WaitForChanges());
        }

    }

    private bool MoveTile(Tile Tile, Vector2Int direction)
    {
        TileCell newCell = null;
        TileCell adjacent = Grid.GetAdjacentCell(Tile.Cell, direction);

        while (adjacent != null)
        {
            if (adjacent.Occupied)
            {
                if (CanMerge(Tile, adjacent.Tile))
                {
                    Merge(Tile, adjacent.Tile);
                    return true;
                }
                break;
            }

            newCell = adjacent;
            adjacent = Grid.GetAdjacentCell(adjacent, direction);
        }

        if (newCell != null)
        {

            // Tile.MoveTo(newCell);
            Tile.MoveTo(newCell, () =>
            {
                waiting = false;
            });
            return true;
        }

        return false;
    }

    private bool CanMerge(Tile a, Tile b)
    {
        return a.State == b.State && !b.Locked;
    }

    private void Merge(Tile a, Tile b)
    {
        Tiles.Remove(a);
        a.Merge(b.Cell, () =>
        {
            int index = Mathf.Clamp(IndexOfState(b.State) + 1, 0, TileStates.Length - 1);
            TileState newState = TileStates[index];

            b.SetState(newState);

            GameManager.IncreaseScore(newState.Number);

            waiting = false;
        });
    }

    private int IndexOfState(TileState state)
    {
        for (int i = 0; i < TileStates.Length; i++)
        {
            if (state == TileStates[i])
            {
                return i;
            }
        }
        return -1;
    }

    private IEnumerator WaitForChanges()
    {
        waiting = true;

        yield return new WaitForSeconds(0.15f);

        waiting = false;

        foreach (var Tile in Tiles)
        {
            Tile.Locked = false;
        }

        if (Tiles.Count != Grid.Size)
        {
            CreateTile();
        }

        if (CheckForGameOver())
        {
            GameManager.GameOver();
        }


    }

    private bool CheckForGameOver()
    {
        if (Tiles.Count != Grid.Size)
        {
            return false;
        }

        foreach (var Tile in Tiles)
        {
            TileCell Up = Grid.GetAdjacentCell(Tile.Cell, Vector2Int.up);
            TileCell Down = Grid.GetAdjacentCell(Tile.Cell, Vector2Int.down);
            TileCell Left = Grid.GetAdjacentCell(Tile.Cell, Vector2Int.left);
            TileCell Right = Grid.GetAdjacentCell(Tile.Cell, Vector2Int.right);

            if (Up != null && CanMerge(Tile, Up.Tile)) return false;
            if (Down != null && CanMerge(Tile, Down.Tile)) return false;
            if (Left != null && CanMerge(Tile, Left.Tile)) return false;
            if (Right != null && CanMerge(Tile, Right.Tile)) return false;
        }

        return true;
    }
}
