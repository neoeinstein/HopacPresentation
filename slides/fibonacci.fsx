(**
- title : Hopac - Alternatives
- description : Introduction to Hopac and Synchronous Rendezvous
- author : Marcus Griep
- theme : Moon
- transition : dissolve

***
*)
(*** hide ***)
open System
open System.Threading
open System.Collections.Generic
#I "../packages/presentation/Hopac/lib/net45"
#r "Hopac.Core.dll"
#r "Hopac.dll"
open Hopac
open Hopac.Infixes
(**
### The Fibonacci sequence
#### Sequential
*)
(*** hide ***)
let time f =
  let sw = System.Diagnostics.Stopwatch()
  sw.Start()
  let r = f ()
  sw.Stop()
  (r, sw.Elapsed)
(*** define-output:fib ***)
let rec fib n =
    if n < 2L then n
    else fib (n - 1L) + fib (n - 2L)

let seqFibTime = time <| fun () -> fib 42L
(*** include-value:seqFibTime ***)
(**
---
### The Fibonacci sequence
#### Hopac timing
*)
(*** define-output:hfibSlowTime ***)
let rec hfib n = Job.delay <| fun () ->
  if n < 2L then
    Job.result n
  else
    hfib (n-2L) <*> hfib (n-1L)
    >>- fun (x,y) -> (x + y)

let hFibSlowTime = time <| fun () -> run (hfib 42L)
(*** include-value:hFibSlowTime ***)
(**
---
### The Fibonacci sequence
#### Hopac timing
*)
(*** define-output:hfibTime ***)
let rec hfibFast level n = Job.delay <| fun () ->
  if n < 2L then
    Job.result n
  elif n < level then Job.result <| fib n
  else
    hfibFast level (n-2L) <*> hfibFast level (n-1L)
    >>- fun (x,y) -> (x + y)

let hFibFastL9Time = time <| fun () -> run (hfibFast 9L 42L)
let hFibFastL18Time = time <| fun () -> run (hfibFast 18L 42L)
(*** include-value:hFibFastL9Time ***)
(*** include-value:hFibFastL18Time ***)
(**
---
### The Fibonacci sequence
#### Hopac job count
*)
(*** define-output:hfibCount ***)
let rec hfibFastWithCount level (c,n) = Job.delay <| fun () ->
  if n < 2L then
    Job.result (c + 1, n)
  elif n < level then Job.result <| (c + 1, fib n)
  else
    hfibFastWithCount level (c,n-2L) <*> hfibFastWithCount level (c,n-1L)
    >>- fun ((cx,x), (cy,y)) -> (cx + cy + 1, x + y)

let hFibFastL9Count =
  fst (run <| hfibFastWithCount 9L (0,42L))
(*** include-value:hFibFastL9Count ***)
(*** hide ***)
let singleSched = Scheduler.create {Scheduler.Create.Def with NumWorkers = Some 1}
let hFibFastL18OneTime =
  time <| fun () ->
      Scheduler.startIgnore singleSched (hfibFast 18L 42L)
      Scheduler.wait singleSched
(**
---
### Fibonacci Comparison

<div class="span5">

#### Sequential
*)
(*** include-value: snd seqFibTime ***)
(**
#### Hopac
*)
(*** include-value: snd hFibSlowTime ***)
(**
</div>
<div class="span5">

#### Hopac (Seq: n < 9)
*)
(*** include-value: snd hFibFastL9Time ***)
(**
#### Hopac (Seq: n < 18)
*)
(*** include-value: snd hFibFastL18Time ***)
(**
#### Hopac (Seq: n < 18, 1 worker)
*)
(*** include-value: snd hFibFastL18OneTime ***)
(**
</div>

---
### Server Garbage Collection
*)
(*** define-output:gcServer ***)
System.Runtime.GCSettings.IsServerGC
(*** include-it:gcServer ***)
(**

    [lang=xml]
    <configuration>
      <runtime>
        <gcServer enabled="true"/>
      </runtime>
    </configuration>

More info on [MSDN](https://msdn.microsoft.com/en-us/library/ms229357%28v=vs.110%29.aspx)

***
### [Agenda](index.html)

1. [Concurrency](concurrency.html)
2. [Hopac](hopac.html)
3. [Alternatives](alternatives.html)
 - [Fibonacci Sequence](fibonacci.html)
 - <span class="nextsegment">[How do they work?](altDeepDive.html)</span>
4. [IVar, MVar, Mailbox...](remainder.html)
*)