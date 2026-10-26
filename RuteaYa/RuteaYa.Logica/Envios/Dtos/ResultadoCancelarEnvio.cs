namespace RuteaYa.Logica.Envios.Dtos
{
    public class ResultadoCancelarEnvio
    {
        public bool ResultadoExitoso { get; set; }
        public bool NoEncontrado { get; set; }
        public string? Error { get; set; }

        public static ResultadoCancelarEnvio Exitoso() =>
            new ResultadoCancelarEnvio { ResultadoExitoso = true };

        public static ResultadoCancelarEnvio NoExiste() =>
            new ResultadoCancelarEnvio { NoEncontrado = true };

        public static ResultadoCancelarEnvio Fallido(string error) =>
            new ResultadoCancelarEnvio { Error = error };
    }
}