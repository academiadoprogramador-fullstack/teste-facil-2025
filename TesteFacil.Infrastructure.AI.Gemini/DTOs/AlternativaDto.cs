﻿namespace TesteFacil.Infrastructure.AI.Gemini.DTOs;

public class AlternativaDto
{
    public string Letra { get; set; }
    public string Resposta { get; set; }
    public bool Correta { get; set; }

    public AlternativaDto() { }
}
