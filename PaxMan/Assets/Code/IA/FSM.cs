public class FSM {

    private int currentState;
    private int[,] relations;

    public FSM(int _states, int _flags)
    {
        currentState = -1;
        relations = new int[_states, _flags];

        for (int i = 0; i < _states; i++)
        {
            for (int j = 0; j < _flags; j++)
            {
                relations[i, j] = -1;
            }
        }
    }

    public void SetState(int _state)
    {
        currentState = _state;
    }


    public void SetRelation(int _sourceState, int _flag, int _destinaionState)
    {
        relations[_sourceState, _flag] = _destinaionState;
    }

    public void SendEvent(int _flag)
    {
        if (relations[currentState,_flag] != -1)
        {
            currentState = relations[currentState, _flag];
        }
    }

    public int GetState()
    {
        return currentState;
    }

}
