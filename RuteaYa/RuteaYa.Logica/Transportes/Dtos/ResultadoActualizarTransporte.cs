namespace RuteaYa.Logica.Transportes.Dtos
    {

    public class ResultadoActualizarTransporte
        {

        public bool ResultadoExitoso { get; set; }
        public bool NoEncontrado { get; set; }
        public string Error { get; set; } = "";

        public static ResultadoActualizarTransporte Exitoso()
            {

            return new ResultadoActualizarTransporte()
                {
                
                ResultadoExitoso = true
                };
            }

        public static ResultadoActualizarTransporte NoExiste()
            {

            return new ResultadoActualizarTransporte()
                {
                ResultadoExitoso = false,
                NoEncontrado = true,
                Error = "No se encontró el transporte solicitado o no tenés permisos para gestionarlo."
                };
            }

        public static ResultadoActualizarTransporte Fallido(string error)
            {
            
            return new ResultadoActualizarTransporte {
                ResultadoExitoso = false,
                Error = error
                };
            }
        }
    }