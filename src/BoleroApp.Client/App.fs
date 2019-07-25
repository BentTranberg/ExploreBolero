namespace BoleroApp.Client

open Bolero
open Bolero.Remoting
open Bolero.Templating.Client
open Elmish

/// The main application component.
type App() =
    inherit ProgramComponent<Main.Model, Main.Message>()

    override this.Program =
        let remote : Main.RemoteServices =
            {
                books = this.Remote<Books.RemoteService>()
                login = this.Remote<Login.RemoteService>()
            }
        let init _ = Main.Model.init, Main.Model.initCmd
        let update message model = Main.Model.update remote message model
        Program.mkProgram init update Main.View.view
        |> Program.withRouter Main.Router.router
#if DEBUG
        |> Program.withHotReloading
#endif
