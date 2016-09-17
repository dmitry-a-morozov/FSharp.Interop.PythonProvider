#I "../../lib"
#r "Python.Runtime.dll"

open Python.Runtime
open System.Collections.Generic

module Python = 
    let init = lazy PythonEngine.Initialize()

    let inline invoke action = 
        init.Force()
        let token = PythonEngine.AcquireLock()
        try action()
        finally PythonEngine.ReleaseLock(token) 

    type PyObject with
        member this.Cast<'T>(): 'T = invoke <| fun() -> 
            this.AsManagedObject(typeof<'T>) |> unbox

        member this.Name = invoke <| fun() -> 
            assert (this.HasAttr("__name__"))
            this.GetAttr("__name__").Cast<string>()

        member this.IsNotNone = invoke <| fun() -> 
            this.IsTrue()

        member this.IsPublic = this.Name.[0] <> '_'

    let modules() = 
        invoke <| fun () -> 
            let sys = PythonEngine.ImportModule("sys")
            [| for x in sys.GetAttr("modules") |> PyList.AsList -> string x |] 
            
Python.modules()
