module ExploreBolero.Client.DatePickerComp

open System
open Bolero.Html
open BlazorNodaTimeDateTimePicker
// Note: NodaTime is accessed qualified, to avoid conflicts.

// Documentation for DatePicker settings is here ( look for [Parameter] ) :
// https://github.com/nheath99/BlazorNodaTimeDateTimePicker/blob/master/src/BlazorNodaTimeDateTimePicker/DatePicker.razor

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
