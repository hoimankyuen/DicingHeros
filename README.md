# Dicing Heros: A Turn-based Strategy Game with physics

## Introduction

![alt text](Screenshots/StartScreen.png?raw=true)
Dicing Heros is a turn-based strategy game about fantasy battles, with the twist that both dice and pieces exist in the same physical space. It is inspired by how the dice thrown in a board game may accidentally interact with other pieces on the board. The game puts the player in command of a team of different units with different stats and abilities, and uses the units to fight the enemy team.
This game is another experiment to explore the concept of a gridless chess-like strategy game, where pieces may be pushed off the grid by thrown dice, adding another layer of variables to the game.

## Gameplay Design

The game takes place on a grided board, where each player/cpu take turns to control their units. Each player can perform the following action during each turn:
- Throwing Dice: The player can select any amount of dice that they own and throw them onto the board. Thrown dice may knock units out of place allow them to occupy more than a tile.
- Activate Equipments: The player can asign dice to the equipments of their units, increasing the capability of said unit.
- Move Units: The player can move units to another tile, given that the tile is within movement range. Dice assigned to movement equipments will be consumed when activated.
- Attack Enemy Units: The player can initiate an attack by their unit attacking enemy units, given that the enemy unit is within attack range. Dice assigned to attack equipments will be comsumed when activated. 

The player team consist of different type of units, each have different capabilities and weakness. The current list of units are as follows:

|      Unit |                                                                          Description | 
|----------:|--------------------------------------------------------------------------------------|
|       Pawn|       Fodder unit. Has weak stats, but can still do well when equipment is activated.|
|     Bishop|       Magic caster. Has strong but expensive spells, but has low health and movement.|
|     Castle| Warrior. Has strong and affordable attacks. Is slow unless using equipement to move. |

Each units wields a set of different equipments, which requires dice to activate. The current list of equipments are as follows:

|       Equipment | Dice Requirements |                                                           Description | 
|----------------:|------------------:|-----------------------------------------------------------------------|
|     Simple Shoe |              >= 2 |                                                         + 1 Movement. |
|     Short Sword |              >= 3 |                                         + 4 Physical Damage, 1 Range. |
|    Small Shield |              >= 2 |                                                          + 2 Defence. |
|     Mage's Ring |         Even, Odd |                                + 3 Magical Damage, + 1 Magical Range. |
|        Fireball |              >= 7 |  + 8 Magical Damage, 3 Magical Range. Diagonal also counts for range. |
|     Thunderbolt |        <= 1, >= 6 | + 6 Magical Damage, 5 Magical Range. Only attack cardinal directions. |
| Quickman's Boot |              >= 5 |                                                         + 4 Movement. |
|     Great Sword |              >= 5 |                                                 + 6 Physical Attacks. |
|        Long Bow |              >= 4 |                                + 3 Physical Damage, 3 Physical Range. |

An AI is implemented as the enemy player. The AI would be playing using the same ruleset as the player, performing the same actions the player can perform. The AI will attempt to do the followings:
- Throw dice. The AI will always throws dice once on each turn.
- Attack units of the player with the intend to fully defeat the unit, consuming dice in the process.
- Attack units of the player with the most damaging attack currently possible, consuming dice in the process.
- Move towards the units of the player.

## UI

The following section describes the every UI on each stages of the game.

![alt text](Screenshots/UnitSelection_Annotated.png?raw=true)
Unit Selection UI
| Number |               Description |
|-------:|---------------------------|
|       1|                       Unit|
|       2|           Unit List Window|
|       3|                        Die|
|       4|            Die List Window|
|       5|            Select All Dice|
|       6|                Information|
|       7|       End the current turn|
|       8| Selected Unit for Movement|
|       9|              Moveable Area|
|      10|            Attackable Area|

![alt text](Screenshots/DiceThrow_Annotated.png?raw=true)
Movement UI
| Number |          Description |
|--------|----------------------|
|       1| Throw Target Position|
|       2|        Throw Strength|


![alt text](Screenshots/Movement_Annotated.png?raw=true)
Attack UI
| Number |                               Description |
|-------:|-------------------------------------------|
|       1|                      Selected Unit Details|
|       2|                        Selected Unit Stats|
|       3|                 Equipment of Selected Unit|
|       4|                                  Die Slot |
|       5|                Skip the turn for this unit|
|       6|               Change to Movement Selection|
|       7|                 Change to Attack Selection|
|       8|                  Go back to Unit Selection|
|       9|                             Movement range|
|      10|                    CUrrently Selected Path|
|      11|                            Possible Target|

![alt text](Screenshots/Attack_Annotated.png?raw=true)
Attack UI
| Number |                               Description |
|-------:|-------------------------------------------|
|       1| Enemy Unit Selection and Damage Calcuation|
|       2|                     Enemy Unit Information|
