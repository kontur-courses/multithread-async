var dict = new Dictionary<string, string>();
dict["test"] = "test";
 
var t = new Thread(() =>
{
    while(true)
        dict[Guid.NewGuid().ToString()] = "test";
});
t.Start();
 
int failedAfter = 0;
string value;
try
{
    while(true)
    {
        value = dict["test"];
        failedAfter++;
    }
}
catch(Exception e)
{
    Console.WriteLine($"Failed after {failedAfter} iterations", e);
}