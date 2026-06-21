# Overview 
This project is a platform game made in Unity with a custom movement controller. The main aim was to create rewarding mechanics which have a learning curve in the game. 

The core gameplay mechanic revolves around platforms reacting only when the player falls. The game makes use of a long coyote time so that the player can intentionally fall of the platform, trigger its behaviour, and still recover by jumping back to safety.

## Gameplay Features
### Custom Jump Tuning

### Dash System
- Multi-directional dash
- Configurable dash decay
- Momentum carry-over after dash completion
- Directional influence while preserving dash momentum

### Dynamic Platform Mechanic
- Platforms react when the player falls from them
- Platform behaviour is tied to player state

---

## Technical
### Coyote Time
Allows jumps after leaving a platform.
Coyote time lasts longer than usual to allow the platform states to be triggered for longer, such as moving towards the player

### Jump Buffering
Stores jump input briefly before landing.
Prevents missed jumps due to timing precision.

### Dash System
The dash system applies an initial velocity and gradually decays that velocity over time.

Momentum is preserved after dash completion and dash cancellation through jumping. This creates a new element to the game as players can jump cancel early or jump cancel into ramps to make longer leaps. 
The player regains directional control when dash has ended and momentum is being carried. For instance there is a faster decay when the player actively moves against momentum.

### Direct Velocity Control
The controller directly manipulates velocity rather than relying on AddForce(). This was implemented to achieve more deterministic movement behaviour and precise control over mechanics such as dash momentum, coyote time, jump buffering, and custom gravity. By expressing movement in terms of desired velocity rather than accumulated forces, the core movement logic is also less dependent on Unity's physics simulation and could be adapted more easily to a custom character controller in future.

### Custom Gravity
Gravity is manually applied rather than using Unitys default gravity behaviour. This gave me better interaction with dash mechanics, a greater control over jump feel, adjustable fall speed limits and more consistent behaviour across movement states.

### Dynamic Platform Behaviour
Platforms react to player state rather than operating independently.

When the player falls from a platform:
There is an interaction between movement systems and environment systems
Coyote-time logic triggers the state transition.
Platforms begin moving.

---

## Future Improvements
### Architecture 
This prototype prioritised iteration on gameplay feel and movement mechanics. As a result, player movement, jumping, and dash behaviour are currently implemented within a single controller script.

For a larger project, I would alter this into a more modular architecture using a Finite State Machine (FSM) to separate movement states (Grounded, Airborne, Dashing, Momentum Carry), along with dedicated movement and input components. This would improve maintainability and scalability as additional mechanics are introduced.

### Gameplay
I would like to further explore the concept of platform states being triggered by coyote time, such as the platforms only reveiling themselves when the player falls off the initial platform. 
