namespace ExploreBolero.Client.BlazorDates

open System
open ExploreBolero.Client

type Message =
    | SetFromDate of DateTime
    | SetToDate of DateTime

type Model =
    {
        fromDate: DateTime
        toDate: DateTime
    }

module Model =

    open Bolero.Remoting.Client
    open Elmish

    let init =
        {
            fromDate = DateTime.Today
            toDate = DateTime.Today
        }

    let update (message: Message) (model: Model) =
        match message with
        | SetFromDate value -> { model with fromDate = value }, Cmd.none
        | SetToDate value -> { model with toDate = value }, Cmd.none

module View =

    open Bolero
    open Bolero.Html
    open Elmish

    type Tmpl = Template<"BlazorDates/blazordates.html">

    let showDate (date: DateTime) = date.ToString "d MMM yyyy"

    let page (model: Model) (dispatch: Dispatch<Message>) =
        let selectFromDate = DatePickerComp.datePicker model.fromDate (fun d -> d |> SetFromDate |> dispatch)
        let selectToDate = DatePickerComp.datePicker model.toDate (fun d -> d |> SetToDate |> dispatch)
        Tmpl()
            .FromDate(selectFromDate)
            .ShowFromDate(showDate model.fromDate)
            .ToDate(selectToDate)
            .ShowToDate(showDate model.toDate)
            .Elt()
