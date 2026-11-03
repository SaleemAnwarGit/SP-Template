namespace StreamProcessor.Pipeline;
using StreamProcessor.Infrastructure; using StreamProcessor.Models;
public sealed class InvariantMonitor
{
    private readonly IPipelineBuffer<SensorReading> _in,_v;
    private readonly List<ProcessingResult> _store; private readonly object _storeLock;
    private volatile bool _running; private Thread? _t;
    public int ViolationCount{get;private set;}
    public InvariantMonitor(IPipelineBuffer<SensorReading> i,IPipelineBuffer<SensorReading> v,List<ProcessingResult> s,object sl){_in=i;_v=v;_store=s;_storeLock=sl;}
    public void Start(){_running=true;_t=new Thread(Run){IsBackground=true,Name="Monitor"};_t.Start();}
    public void Stop(){_running=false;_t?.Join(500);}
    private void Run(){while(_running){Check(_in,"in");Check(_v,"valid");List<ProcessingResult> snap;lock(_storeLock){snap=_store.ToList();}foreach(var r in snap)if(r.NormalizedValue<0||r.NormalizedValue>1){ViolationCount++;ConcurrentLogger.Log("MONITOR","VIOLATION",$"Norm={r.NormalizedValue:F4}");}Thread.Sleep(10);}}
    private void Check(IPipelineBuffer<SensorReading> b,string n){int c=b.Count;if(c>b.Capacity){ViolationCount++;ConcurrentLogger.Log("MONITOR","VIOLATION",$"{n}={c}>{b.Capacity}");}}
}
