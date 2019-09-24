module ExploreBolero.Client.DatePickerComp

open System
open Bolero.Html
open NodaTimePicker
// Note: NodaTime is accessed qualified, to avoid conflicts.

// NOTE!!! The following is for the old library, named BlazorNodaTimeDateTimePicker.
// Documentation for DatePicker settings is here ( look for [Parameter] ) :
// https://github.com/nheath99/BlazorNodaTimeDateTimePicker/blob/master/src/BlazorNodaTimeDateTimePicker/DatePicker.razor
// NOTE!!! DEAD LINK

// This is the repo for the new library:
// https://github.com/nheath99/NodaTimePicker

let datePicker (selectedDate: DateTime) (handleSelected: DateTime -> unit) =
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
        "Selected" => Action<_>(fun (d: NodaTime.LocalDate) -> d.ToDateTimeUnspecified() |> handleSelected)
    ] []
