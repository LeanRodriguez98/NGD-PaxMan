public class FSM {

    private int currentState;
    private int[,] relations;

    public FSM(int states, int flags)
    {
        currentState = -1;
        relations = new int[states, flags];

        for (int i = 0; i < states; i++)
        {
            for (int j = 0; j < flags; j++)
            {
                relations[i, j] = -1;
            }
        }
    }

    public void SetState(int state)
    {
        currentState = state;
    }


    public void SetRelation(int sourceState, int flag, int destinaionState)
    {
        relations[sourceState, flag] = destinaionState;
    }

    public void SendEvent(int flag)
    {
        if (relations[currentState,flag] != -1)
        {
            currentState = relations[currentState, flag];
        }
    }

    public int GetState()
    {
        return currentState;
    }

}
