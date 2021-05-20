module TemplateSelector.Program

open System
open Elmish
open Elmish.WPF

module Report =
  type Model =
    {
      Header: string
      LotsOfStuff: string list
    }

  type Msg =
    | AddStuff

  let init() =
    { Header = "The report header"
      LotsOfStuff = [ "Line 1"; "Line 2" ] }

  let update msg m =
    match msg with
    | AddStuff -> { m with LotsOfStuff = m.LotsOfStuff @ [ "Another line" ] }

  let bindings () : Binding<Model, Msg> list = [
    "ExtendList" |> Binding.cmd AddStuff
    "Header" |> Binding.oneWay (fun m -> m.Header)
  ]

  let reportVm = ViewModel.designInstance(init())(bindings())

module Input =
  type Model =
    {
      StartDate: DateTime
    }

  type Msg =
    | Earlier
    | Later

  let init() =
    { StartDate = DateTime.Today
    }

  let update msg m =
    match msg with
    | Earlier -> { m with StartDate = m.StartDate.AddDays(-1.0) }
    | Later -> { m with StartDate = m.StartDate.AddDays(1.0) }

  let bindings () : Binding<Model, Msg> list = [
    "GoBack" |> Binding.cmd Earlier
    "GoForward" |> Binding.cmd Later
    "StartDate" |> Binding.oneWay (fun m -> m.StartDate )
  ]

module Pane =
  type Panes =
    | ShowReport of Report.Model
    | GatherInput of Input.Model

  type Model =
    {
      Valid: bool
      Specific: Panes
    }

  type Msg =
    | MarkValid
    | MarkInValid
    | ReportMsg of Report.Msg
    | InputMsg of Input.Msg

  let init() =
    { Valid = false
      Specific = GatherInput (Input.init()) }

  let update msg m =
    match msg with
    | MarkValid -> { m with Valid = true }
    | MarkInValid -> { m with Valid = false }
    | ReportMsg reportMsg ->
                 match m.Specific with
                 | ShowReport model -> { m with Specific = ShowReport (Report.update reportMsg model) }
                 | _ -> failwith "Can't send ReportMsg to non-Report"
    | InputMsg inputMsg ->
                 match m.Specific with
                 | GatherInput inputModel -> { m with Specific = GatherInput (Input.update inputMsg inputModel) }
                 | _ -> failwith "Can't send InputMsg to non-Input"

  let bindings() : Binding<Model, Msg> list = [
    "IsValid" |> Binding.oneWay (fun m -> m.Valid)
    "ReportModel" |> Binding.subModel(
                      (fun m -> match m.Specific with
                                | ShowReport reportModel -> reportModel
                                | _ -> Report.init()),
                      snd,
                      ReportMsg,
                      (fun () -> Report.bindings())
                    )
    "InputModel" |> Binding.subModel(
                      (fun m -> match m.Specific with
                                | GatherInput inputModel -> inputModel
                                | _ -> Input.init()),
                      snd,
                      InputMsg,
                      (fun () -> Input.bindings())
                    )
  ]

module App =
  type Model =
    { GlobalState: bool
      CurrentPane: Pane.Model }

  type Msg =
    | ToggleGlobalState
    | PaneMsg of Pane.Msg
    | SetInputPanel
    | SetReportPanel

  let init() =
    { GlobalState = false
      CurrentPane = Pane.init() }

  let update msg m =
    match msg with
    | ToggleGlobalState -> { m with GlobalState = not m.GlobalState }
    | PaneMsg paneMsg -> { m with CurrentPane = Pane.update paneMsg m.CurrentPane }
    | SetInputPanel -> { m with CurrentPane = {
                            Valid = m.CurrentPane.Valid
                            Specific = Pane.Panes.GatherInput (Input.init()) } }
    | SetReportPanel -> { m with CurrentPane = {
                             Valid = m.CurrentPane.Valid
                             Specific = Pane.Panes.ShowReport (Report.init()) } }

  let bindings () : Binding<Model, Msg> list = [
    "Toggle" |> Binding.cmd ToggleGlobalState
    "GatherInput" |> Binding.cmd SetInputPanel
    "ShowReport" |> Binding.cmd SetReportPanel
    "PaneType" |> Binding.oneWay (fun m -> m.CurrentPane)
    "Pane" |> Binding.subModel(
      (fun m -> m.CurrentPane),
      snd,
      PaneMsg,
      (fun () -> Pane.bindings()))
  ]

  let main window =
    Program.mkSimpleWpf init update bindings
    |> Program.withConsoleTrace
    |> Program.startElmishLoop
      { ElmConfig.Default with LogConsole = true; Measure = true }
      window
