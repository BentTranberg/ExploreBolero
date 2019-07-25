namespace BoleroApp.Server.RemoteServices

open System.IO
open Microsoft.AspNetCore.Hosting
open Bolero
open Bolero.Remoting.Server
open BoleroApp.Client

// This remote service requires injected dependencies (namely, `env`),
// so it must be defined as a RemoteHandler class.
type BookService(env: IWebHostEnvironment) =
    inherit RemoteHandler<Books.RemoteService>()

    let books =
        Path.Combine(env.ContentRootPath, "data/books.json")
        |> File.ReadAllText
        |> Json.Deserialize<Books.Book[]>
        |> ResizeArray

    override this.Handler =
        {
            getBooks = Remote.authorize <| fun _ () -> async {
                return books.ToArray()
            }

            addBook = Remote.authorize <| fun _ book -> async {
                books.Add(book)
            }

            removeBookByIsbn = Remote.authorize <| fun _ isbn -> async {
                books.RemoveAll(fun b -> b.isbn = isbn) |> ignore
            }
        }
