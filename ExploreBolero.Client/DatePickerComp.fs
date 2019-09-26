module ExploreBolero.Client.DatePickerComp

open System
open Bolero.Html
open NodaTimePicker
open Microsoft.AspNetCore.Components
// Note: NodaTime is accessed qualified, to avoid conflicts.

// Documentation for DatePicker
// https://github.com/nheath99/NodaTimePicker/blob/master/src/NodaTimePicker/DatePickerComponentBase.cs

type NodaLocalDateDelegate = delegate of NodaTime.LocalDate -> unit

let datePicker (selectedDate: DateTime) (handleSelected: DateTime -> unit) =
    let myHandler (d: NodaTime.LocalDate) = d.ToDateTimeUnspecified() |> handleSelected
    let myDelegate = new NodaLocalDateDelegate(myHandler)
    let OnSelected = new EventCallback<NodaTime.LocalDate>(null, myDelegate)
    comp<DatePicker> [
        "Inline" => true
        "SelectedDate" => NodaTime.LocalDate.FromDateTime selectedDate
        "ShowClear" => false
        "DisplayWeekNumber" => true

        // "FirstDayOfWeek" => NodaTime.IsoDayOfWeek.Monday // default: Monday

        // Localization settings. Settings for Norwegian culture and language.
        // "FormatProvider" => new Globalization.CultureInfo "nb-NO" // default: Globalization.CultureInfo.InvariantCulture
        // "TodayText" => "Dags dato" // default: Today
        // "CloseText" => "Lukk" // default: Close
        // "WeekAbbreviation" => "Uke" // default: Wk

        // Handlers
        "OnSelected" => OnSelected
    ] []
