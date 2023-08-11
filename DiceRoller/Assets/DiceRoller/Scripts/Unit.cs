using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuickerEffects;
using UnityEngine;

namespace DiceRoller
{
    public class Unit : MonoBehaviour
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
            RegisterStateBehaviours();
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
                DeregisterStateBehaviours();
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

        protected void RegisterStateBehaviours()
        {
            game.RegisterStateBehaviour(this, State.Navigation, new NavitigationStateBehaviour(this));
            game.RegisterStateBehaviour(this, State.UnitMovementSelection, new UnitMovementSelectionStateBehaviour(this));
            game.RegisterStateBehaviour(this, State.UnitMovement, new UnitMovementStateBehaviour(this));
        }

        protected void DeregisterStateBehaviours()
        {
            game.DeregisterStateBehaviour(this);
        }

        // ========================================================= Navigation State =========================================================

        protected class NavitigationStateBehaviour : IStateBehaviour
        {
            Unit unit = null;
            protected List<Tile> navigationInTiles = new List<Tile>();

            public NavitigationStateBehaviour(Unit obj) { this.unit = obj; }

            public void OnStateEnter()
            {
                unit.outline.Show = unit.isHovering;
                List<Tile> tiles = unit.isHovering ? unit.InTiles : unit.emptyTileList;

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

            public void OnStateUpdate()
            {
                unit.outline.Show = unit.isHovering;
                List<Tile> tiles = unit.isHovering ? unit.InTiles : unit.emptyTileList;

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

            public void OnStateExit()
            {
                unit.outline.Show = false;
                foreach (Tile tile in navigationInTiles)
                {
                    tile.RemoveDisplay(this, Tile.DisplayType.Position);
                }
                navigationInTiles.Clear();
            }
        }

        // ========================================================= Unit Movement Selection State =========================================================

        class UnitMovementSelectionStateBehaviour : IStateBehaviour
        {
            protected Unit unit = null;

            protected List<Tile> unitMovementInTiles = new List<Tile>();
            protected List<Tile> unitMovementMoveTiles = new List<Tile>();
            protected List<Tile> unitMovementPathTiles = new List<Tile>();
            protected Tile unitMovementHitTile = null;
            protected bool unitMovementStartedPress = false;

            public UnitMovementSelectionStateBehaviour(Unit unit) { this.unit = unit; }

            public void OnStateEnter()
            {
                if ((Object)unit.game.StateParams[0] == unit)
                {
                    unit.outline.Show = true;
                    foreach (Tile tile in unit.InTiles)
                    {
                        tile.AddDisplay(this, Tile.DisplayType.Position);
                    }
                    unitMovementInTiles.AddRange(unit.InTiles);

                    foreach (Tile tile in unit.board.GetTileWithinRange(unit.InTiles, unit.movement))
                    {
                        tile.AddDisplay(this, Tile.DisplayType.Move);
                    }
                    unitMovementMoveTiles.AddRange(unit.board.GetTileWithinRange(unit.InTiles, unit.movement));
                }
            }

            public void OnStateUpdate()
            {
                if ((Object)unit.game.StateParams[0] == unit)
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
                            foreach (Tile tile in unitMovementPathTiles)
                            {
                                tile.HidePath();
                            }
                            unitMovementHitTile.RemoveDisplay(this, Tile.DisplayType.MoveTarget);
                        }
                        if (hitTile != null)
                        {
                            unitMovementPathTiles.Clear();
                            unitMovementPathTiles.AddRange(unit.board.GetShortestPath(unit.InTiles, hitTile));
                            foreach (Tile tile in unitMovementPathTiles)
                            {
                                tile.ShowPath(unitMovementPathTiles);
                            }
                            hitTile.AddDisplay(this, Tile.DisplayType.MoveTarget);
                        }
                        else
                        {
                            unitMovementPathTiles.Clear();
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
                            unit.game.ChangeState(State.UnitMovement, unit, new List<Tile>(unitMovementPathTiles));
                        }
                        else
                        {
                            unit.game.ChangeState(State.Navigation);
                        }
                        unitMovementStartedPress = false;
                    }
                }
            }

            public void OnStateExit()
            {
                if ((Object)unit.game.StateParams[0] == unit)
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

                    foreach (Tile tile in unitMovementPathTiles)
                    {
                        tile.HidePath();
                    }
                    unitMovementPathTiles.Clear();

                    if (unitMovementHitTile != null)
                    {
                        unitMovementHitTile.RemoveDisplay(this, Tile.DisplayType.MoveTarget);
                    }
                    unitMovementHitTile = null;
                }
            }

        }


        // ========================================================= Unit Movement State =========================================================

        class UnitMovementStateBehaviour : IStateBehaviour
        {
            protected Unit unit = null;

            public UnitMovementStateBehaviour(Unit unit) { this.unit = unit; }

            public void OnStateEnter()
            {
                if ((Object)unit.game.StateParams[0] == unit)
                {
                    unit.StartCoroutine(MoveUnitCoroutine((List<Tile>)unit.game.StateParams[1]));
                }
            }

            protected IEnumerator MoveUnitCoroutine(List<Tile> path)
            {
                List<Tile> startTiles = new List<Tile>(unit.InTiles);
                startTiles.ForEach(x => x.AddDisplay(this, Tile.DisplayType.Position));
                path.ForEach(x => { x.AddDisplay(this, Tile.DisplayType.Move); x.ShowPath(path); });
                path[path.Count - 1].AddDisplay(this, Tile.DisplayType.MoveTarget);

                List<Vector3> pathPos = path.Select(x => x.transform.position).ToList();

                if (Vector3.Distance(unit.transform.position, pathPos[0]) > 0.01f)
                {
                    Vector3 startPosition = unit.transform.position;
                    float startTime = Time.time;
                    float duration = Vector3.Distance(startPosition, pathPos[0]) / path[0].tileSize * 0.25f;
                    while (Time.time - startTime <= duration)
                    {
                        unit.rigidBody.MovePosition(Vector3.Lerp(startPosition, pathPos[0], (Time.time - startTime) / duration));
                        yield return new WaitForFixedUpdate();
                    }
                }

                for (int i = 0; i < pathPos.Count - 1; i++)
                {
                    float startTime = Time.time;
                    while (Time.time - startTime <= 0.25f)
                    {
                        unit.rigidBody.MovePosition(Vector3.Lerp(pathPos[i], pathPos[i + 1], (Time.time - startTime) / 0.25f));
                        yield return new WaitForFixedUpdate();
                    }
                }
                unit.rigidBody.MovePosition(pathPos[pathPos.Count - 1]);
                yield return new WaitForFixedUpdate();

                startTiles.ForEach(x => x.RemoveDisplay(this, Tile.DisplayType.Position));
                path.ForEach(x => { x.RemoveDisplay(this, Tile.DisplayType.Move); x.HidePath(); });
                path[path.Count - 1].RemoveDisplay(this, Tile.DisplayType.MoveTarget);
                unit.game.ChangeState(State.Navigation);
            }

            public void OnStateUpdate()
            {
            }

            public void OnStateExit()
            {
            }
        }


        // ========================================================= Apparence =========================================================


    }
}