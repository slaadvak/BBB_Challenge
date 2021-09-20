namespace ThreadUtils
{

public abstract class Worker
{
    //This method will be called when the thread is started.
    public abstract void DoWork();

    public void RequestStop()
    {
        _shouldStop = true;
    }
    //Volatile is used as hint to the compiler that this data
    //member will be accessed by multiple threads.
    protected volatile bool _shouldStop;
}

}   // ns ThreadUtils