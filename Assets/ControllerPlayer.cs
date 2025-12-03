using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ControllerPlayer : MonoBehaviour
{
    [Header("Configuración Visual")]
    public Sprite imagenActiva;   // Arrastra aquí tu sprite Naranja
    public Sprite imagenInactiva; // Arrastra aquí tu sprite Azul

    [Header("Mis Botones")]
    public List<Button> botonesDeLaFila; // Arrastra aquí todos los botones de esta fila

    [Header("Dato Seleccionado")]
    public string opcionElegida; // Aquí se guardará qué eligió el usuario

    void Start()
    {
        // Opcional: Asegurarnos de que los botones sepan qué hacer al inicio
        foreach (Button btn in botonesDeLaFila)
        {
            // Le añadimos la función "AlHacerClick" dinámicamente a cada botón
            btn.onClick.AddListener(() => AlHacerClick(btn));
        }
    }

    // Esta función se ejecuta cuando tocas CUALQUIER botón de la lista
    void AlHacerClick(Button botonOprimido)
    {
        // 1. Guardar el nombre de lo que eligió
        opcionElegida = botonOprimido.name; 
        Debug.Log("Elegiste: " + opcionElegida);

        // 2. Bucle: Revisar todos los botones para pintar los colores correctos
        foreach (Button btn in botonesDeLaFila)
        {
            if (btn == botonOprimido)
            {
                // Si es el que toqué -> Naranja
                btn.image.sprite = imagenActiva;
            }
            else
            {
                // Si NO es el que toqué -> Azul
                btn.image.sprite = imagenInactiva;
            }
        }
    }
}