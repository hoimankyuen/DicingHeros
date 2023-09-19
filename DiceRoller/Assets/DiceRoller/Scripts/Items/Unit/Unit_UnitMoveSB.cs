using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiceRoller
{
    public partial class Unit
    {
		class UnitMoveSB : StateBehaviour
		{
			protected readonly Unit self = null;

			/// <summary>
			/// Constructor.
			/// </summary>
			public UnitMoveSB(Unit self)
			{
				this.self = self;
			}

			/// <summary>
			/// OnStateEnter is called when the centralized state machine is entering the current state.
			/// </summary>
			public override void OnStateEnter()
			{
				// show depleted effect
				if (self.ActionDepleted)
				{
					self.AddEffect(StatusType.Depleted);
				}

				// execute only if the moving unit is this unit
				if (self.IsSelected)
				{
					self.StartCoroutine(MoveSequence());
				}
			}

			/// <summary>
			/// Movement coroutine for this unit.
			/// </summary>
			protected IEnumerator MoveSequence()
			{
				List<Tile> startTiles = self.MovementStartingTiles;
				List<Tile> path = self.MovementSelectedPath;

				// show all displays on grid
				startTiles.ForEach(x => x.UpdateDisplayAs(self, Tile.DisplayType.SelfPosition, startTiles));
				path.ForEach(x => { x.UpdateDisplayAs(self, Tile.DisplayType.Move, path); x.ShowPath(path); });
				path[path.Count - 1].UpdateDisplayAs(self, Tile.DisplayType.MoveTarget, path[path.Count - 1]);

				// show unit info on ui
				float startTime = 0;
				float duration = 0;

				// move to first tile on path if needed
				if (Vector3.Distance(self.transform.position, path[0].transform.position) > 0.01f)
				{
					Vector3 startPosition = self.transform.position;
					startTime = Time.time;
					duration = Vector3.Distance(startPosition, path[0].transform.position) / path[0].tileSize * self.moveTimePerTile;
					while (Time.time - startTime <= duration)
					{
						self.rigidBody.MovePosition(Vector3.Lerp(startPosition, path[0].transform.position, (Time.time - startTime) / duration));
						yield return new WaitForFixedUpdate();
					}
				}

				// move to next tile on path one by one
				for (int i = 0; i < path.Count - 1; i++)
				{
					startTime = Time.time;
					duration = self.moveTimePerTile;
					while (Time.time - startTime <= self.moveTimePerTile)
					{
						self.rigidBody.MovePosition(Vector3.Lerp(path[i].transform.position, path[i + 1].transform.position, (Time.time - startTime) / duration));
						yield return new WaitForFixedUpdate();
					}
				}

				// file tile on path reached, final movement to destination tile 
				startTime = Time.time;
				duration = self.moveTimePerTile;
				while (Time.time - startTime <= self.moveTimePerTile)
				{
					self.rigidBody.MovePosition(path[path.Count - 1].transform.position);
					yield return new WaitForFixedUpdate();
				}

				// hide all displays on grid
				startTiles.ForEach(x => x.UpdateDisplayAs(self, Tile.DisplayType.SelfPosition, Tile.EmptyTiles));
				path.ForEach(x => { x.UpdateDisplayAs(self, Tile.DisplayType.Move, Tile.EmptyTiles); x.HidePath(); });
				path[path.Count - 1].UpdateDisplayAs(self, Tile.DisplayType.MoveTarget, (Tile)null);

				// set flag
				self.ActionDepleted = true;
				self.AddEffect(StatusType.Depleted);

				// clear information
				self.RemoveFromSelection();
				self.MovementStartingTiles.Clear();
				self.MovementSelectedPath.Clear();

				// change state back to navigation
				stateMachine.ChangeState(State.Navigation);
			}

			/// <summary>
			/// OnStateUpdate is called each frame when the centralized state machine is ing the current state.
			/// </summary>
			public override void OnStateUpdate()
			{
			}

			/// <summary>
			/// OnStateExit is called when the centralized state machine is leaving the current state.
			/// </summary>
			public override void OnStateExit()
			{
				// hide depleted effect
				if (self.ActionDepleted)
				{
					self.RemoveEffect(StatusType.Depleted);
				}
			}
		}
	}
}