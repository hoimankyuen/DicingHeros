using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuickerEffects;
using UnityEngine;

namespace DiceRoller
{
    public class Unit : MonoBehaviour, IStateMachine
    {
        // parameters
        public float size = 1f;
        public int movement = 4;

        // reference
        protected GameController game { get { return GameController.Instance; } }
        protected Board board { get { return Board.Instance; } }

        // components
        Rigidbody rigidBody = null;
        Outline outline = null;

        // working variables
        protected float lastMovingTime = 0;
        protected bool moved = false;

        protected Vector3 lastPosition = Vector3.zero;
        protected List<Tile> emptyTileList = new List<Tile>();
        protected List<Tile> lastInTiles = new List<Tile>();

        protected bool isHovering = false;
        protected bool initatedPress = false;


        // ========================================================= Derived Properties =========================================================
        
        public bool IsMoving => Time.time - lastMovingTime < 0.25f;

        public List<Tile> InTiles => Vector3.Distance(transform.position, lastPosition) < 0.0001f ? lastInTiles : ForceGetInTiles();

        // ========================================================= Monobehaviour Methods =========================================================

        /// <summary>
        /// Awake is called when the game object was created. It is always called before start and is 
        /// independent of if the game object is active or not.
        /// </summary>
        void Awake()
        {
            // retrieve components
            rigidBody = GetComponent<Rigidbody>();
            outline = GetComponent<Outline>();
        }

        /// <summary>
        /// Start is called before the first frame update and/or the game object is first active.
        /// </summary>
        void Start()
        {
            game.RegisterStateMachine(this);
        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        void Update()
        {
            moved = false;
            if (rigidBody.velocity.sqrMagnitude > 0.01f || rigidBody.angularVelocity.sqrMagnitude > 0.01f)
            {
                lastMovingTime = Time.time;
                moved = true;
            }
        }

        /// <summary>
        /// OnDestroy is called when an game object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (game != null)
                game.DeregisterStateMachine(this);
        }

        /// <summary>
        /// OnDrawGizmos is called when the game object is in editor mode
        /// </summary>
        void OnDrawGizmos()
        {
            if (Application.isEditor)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(transform.position, size / 2);
            }
        }

        /// <summary>
        /// OnMouseEnter is called when the mouse is start pointing to the game object.
        /// </summary>
        void OnMouseEnter()
        {
            isHovering = true;
        }

        /// <summary>
        /// OnMouseExit is called when the mouse is stop pointing to the game object.
        /// </summary>
        void OnMouseExit()
        {
            isHovering = false;
            initatedPress = false;
        }

        /// <summary>
        /// OnMouseDown is called when a mouse button is pressed when pointing to the game object.
        /// </summary>
        void OnMouseDown()
        {
            initatedPress = true;
        }

        /// <summary>
        /// OnMouseUp is called when a mouse button is released when pointing to the game object.
        /// </summary>
        void OnMouseUp()
        {
            if (game.CurrentState == State.Navigation)
            {
                if (initatedPress)
                {
                    game.ChangeState(State.UnitMovementSelection, this);
                }
            }
            initatedPress = false;
        }

        // ========================================================= General Behaviour =========================================================

        /// <summary>
        /// Find which tiles this game object is in.
        /// </summary>
        protected List<Tile> ForceGetInTiles()
        {
            lastInTiles.Clear();
            lastInTiles.AddRange(Board.Instance.GetCurrentTiles(transform.position, size));
            return lastInTiles;
        }

        // ========================================================= State Machine Behaviour =========================================================

        /// <summary>
        /// OnStateEnter is called when the state is changed into the current one.
        /// </summary>
        public void OnStateEnter()
        {
            switch (game.CurrentState)
            {
                case State.Navigation:
                    NavigationStateEnter();
                    break;
                case State.UnitMovementSelection:
                    UnitMovementSelectionStateEnter();
                    break;
                case State.UnitMovement:
                    UnitMovementStateEnter();
                    break;
            }
        }

        /// <summary>
        /// OnStateUpdate is called each frame when the state is the current one.
        /// </summary>
        public void OnStateUpdate()
        {
            switch (game.CurrentState)
            {
                case State.Navigation:
                    NavigationStateUpdate();
                    break;
                case State.UnitMovementSelection:
                    UnitMovementSelectionStateUpdate();
                    break;
                case State.UnitMovement:
                    UnitMovementStateUpdate();
                    break;
            }
        }

        /// <summary>
        /// OnStateExit is called when the state is changed from the current one.
        /// </summary>
        public void OnStateExit()
        {
            switch (game.CurrentState)
            {
                case State.Navigation:
                    NavigationStateExit();
                    break;
                case State.UnitMovementSelection:
                    UnitMovementSelectionStateExit();
                    break;
                case State.UnitMovement:
                    UnitMovementStateExit();
                    break;
            }
        }

        // ========================================================= Navigation State =========================================================

        protected List<Tile> navigationInTiles = new List<Tile>();

        protected void NavigationStateEnter()
        {

        }

        protected void NavigationStateUpdate()
        {
            outline.Show = isHovering;
            List<Tile> tiles = isHovering ? InTiles : emptyTileList;

            foreach (Tile tile in tiles.Except(navigationInTiles))
            {
                tile.AddDisplay(this, Tile.DisplayType.Position);
            }
            foreach (Tile tile in navigationInTiles.Except(tiles))
            {
                tile.RemoveDisplay(this, Tile.DisplayType.Position);
            }
            navigationInTiles.Clear();
            navigationInTiles.AddRange(tiles);
        }

        protected void NavigationStateExit()
        {
            outline.Show = false;
            foreach (Tile tile in navigationInTiles)
            {
                tile.RemoveDisplay(this, Tile.DisplayType.Position);
            }
            navigationInTiles.Clear();
        }

        // ========================================================= Unit Movement Selection State =========================================================

        protected List<Tile> unitMovementInTiles = new List<Tile>();
        protected List<Tile> unitMovementMoveTiles = new List<Tile>();
        protected Tile unitMovementHitTile = null;
        protected bool unitMovementStartedPress = false;

        protected void UnitMovementSelectionStateEnter()
        {
            if ((Object)game.StateParams[0] == this)
            {
                outline.Show = true;
                foreach (Tile tile in InTiles)
                {
                    tile.AddDisplay(this, Tile.DisplayType.Position);
                }
                unitMovementInTiles.AddRange(InTiles);


                foreach (Tile tile in board.GetTileWithinRange(InTiles, movement))
                {
                    tile.AddDisplay(this, Tile.DisplayType.Move);
                }
                unitMovementMoveTiles.AddRange(board.GetTileWithinRange(InTiles, movement));
            }
        }

        protected void UnitMovementSelectionStateUpdate()
        {
            if ((Object)game.StateParams[0] == this)
            {
                Tile hitTile = null;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Camera.main.farClipPlane, LayerMask.GetMask("Tile")))
                {
                    Tile tile = hit.collider.GetComponentInParent<Tile>();
                    if (unitMovementMoveTiles.Contains(tile))
                    {
                        hitTile = tile;
                    }
                }

                if (unitMovementHitTile != hitTile)
                {
                    if (unitMovementHitTile != null)
                    {
                        unitMovementHitTile.RemoveDisplay(this, Tile.DisplayType.MoveTarget);
                    }
                    if (hitTile != null)
                    {
                        hitTile.AddDisplay(this, Tile.DisplayType.MoveTarget);
                    }
                }
                unitMovementHitTile = hitTile;

                if (Input.GetMouseButtonDown(0))
                {
                    unitMovementStartedPress = true;
                }
                if (Input.GetMouseButtonUp(0) && unitMovementStartedPress)
                {
                    if (unitMovementHitTile != null)
                    {
                        if (!unitMovementInTiles.Contains(unitMovementHitTile))
                        {
                            game.ChangeState(State.UnitMovement, this, unitMovementHitTile);
                        }
                    }
                    else
                    {
                        game.ChangeState(State.Navigation);
                    }
                    unitMovementStartedPress = false;
                }
            }
        }

        protected void UnitMovementSelectionStateExit()
        {
            if ((Object)game.StateParams[0] == this)
            {
                foreach (Tile tile in unitMovementInTiles)
                {
                    tile.RemoveDisplay(this, Tile.DisplayType.Position);
                }
                unitMovementInTiles.Clear();

                foreach (Tile tile in unitMovementMoveTiles)
                {
                    tile.RemoveDisplay(this, Tile.DisplayType.Move);
                }
                unitMovementMoveTiles.Clear();

                if (unitMovementHitTile != null)
                {
                    unitMovementHitTile.RemoveDisplay(this, Tile.DisplayType.MoveTarget);
                }
                unitMovementHitTile = null;
            }
        }

        // ========================================================= Unit Movement State =========================================================

        protected void UnitMovementStateEnter()
        {
            if ((Object)game.StateParams[0] == this)
            {
                StartCoroutine(MoveUnitCoroutine(((Tile)game.StateParams[1])));
            }
        }

        protected IEnumerator MoveUnitCoroutine(Tile targetTile)
        {
            List<Vector3> path = board.GetShortestPath(InTiles, targetTile).Select(x => x.transform.position).ToList();

            if (Vector3.Distance(transform.position, path[0]) > 0.01f)
            {
                Vector3 startPosition = transform.position;
                float startTime = Time.time;
                float duration = Vector3.Distance(startPosition, path[0]) / targetTile.tileSize * 0.25f;
                while (Time.time - startTime <= duration)
                {
                    rigidBody.MovePosition(Vector3.Lerp(startPosition, path[0], (Time.time - startTime) / duration));
                    yield return new WaitForFixedUpdate();
                }
            }

            for (int i = 0; i < path.Count - 1; i++)
            {
                float startTime = Time.time;
                while (Time.time - startTime <= 0.25f)
                {
                    rigidBody.MovePosition(Vector3.Lerp(path[i], path[i + 1], (Time.time - startTime) / 0.25f));
                    yield return new WaitForFixedUpdate();
                }
            }
            rigidBody.MovePosition(path[path.Count - 1]);
            yield return new WaitForFixedUpdate();

            game.ChangeState(State.Navigation);
        }

        protected void UnitMovementStateUpdate()
        {
        }

        protected void UnitMovementStateExit()
        {
        }

        // ========================================================= Apparence =========================================================


    }
}