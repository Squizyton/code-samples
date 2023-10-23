namespace Player.Movement.States
{
    public abstract class PlayerBaseState
    {
        protected OldPlayerMovement _movement;

        protected PlayerBaseState(OldPlayerMovement oldPlayerMovement)
        {
            _movement = oldPlayerMovement;
        }


        public abstract void EnterState();
        public abstract void UpdateState();
    }
}
