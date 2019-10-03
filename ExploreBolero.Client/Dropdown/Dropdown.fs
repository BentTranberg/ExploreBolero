namespace ExploreBolero.Client.Dropdown

open System

type Message =
    | Dummy

type Model =
    {
        dummy: string
    }

module Model =

    open Bolero.Remoting.Client
    open Elmish

    let init =
        {
            dummy = ""
        }

    let update (message: Message) (model: Model) =
        match message with
        | Dummy -> model, Cmd.none

module View =

    open Bolero
    open Bolero.Html
    open Elmish

    type Tmpl = Template<"Dropdown/dropdown.html">

    let dropdown =
        div [ attr.``class`` "dropdown is-hoverable" ] [
            div [ attr.``class`` "dropdown-trigger" ] [
                button [ attr.``class`` "button"; "aria-haspopup" => "true"; "aria-controls" => "dropdown-menu" ] [
                    span [] [ text "Dropdown button" ]
                    span [ attr.``class`` "icon is-small" ] [
                        i [ attr.``class`` "fas fa-angle-down"; "aria-hidden" => "true" ] []
                    ]
                ]
            ]
            div [ attr.``class`` "dropdown-menu"; attr.id "dropdown-menu"; "role" => "menu" ] [
                div [ attr.``class`` "dropdown-content" ] [
                    a [ attr.``class`` "dropdown-item" ] [ text "One" ]
                    a [ attr.``class`` "dropdown-item" ] [ text "Two" ]
                    a [ attr.``class`` "dropdown-item is-active" ] [ text "Three" ]
                    a [ attr.``class`` "dropdown-item" ] [ text "Four" ]
                    a [ attr.``class`` "dropdown-item" ] [ text "Five" ]
                ]
            ]
        ]

    let page (model: Model) (dispatch: Dispatch<Message>) =
        Tmpl()
            .Dropdown1(dropdown)
            .Dropdown2(dropdown)
            .Elt()
