using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ITV.Dto;
using ITV.Mappers;
using ITV.Models;
using ITV.Service.Citas;
using Serilog;

namespace ITV.ViewModels.Citas;

public partial class CitaFormularioModel : ObservableObject
{
    private readonly ILogger _logger = Log.ForContext<CitaFormularioModel>();

    private readonly IService<int, Cita> _citaService;
    private readonly Cita _citaOriginal;
    private readonly Action _closeAction;

    [ObservableProperty] private string _matricula = string.Empty;
    [ObservableProperty] private string _marca = string.Empty;
    [ObservableProperty] private string _modelo = string.Empty;
    [ObservableProperty] private string _dni = string.Empty;
    [ObservableProperty] private string _cilindrada = string.Empty;
    [ObservableProperty] private string _motorSeleccionado = string.Empty;
    [ObservableProperty] private DateTime? _fechaMatriculacion;
    [ObservableProperty] private DateTime? _fechaInspeccion;

    public ObservableCollection<string> TiposMotor { get; } = new() { "Gasolina", "Diesel", "Hidrogeno", "Electrico" };

    [ObservableProperty] private string _titulo = string.Empty;
    private bool _isNew;
    private string _isoFormat = "s";
    private CultureInfo _invariant = CultureInfo.InvariantCulture;
    
    public CitaFormularioModel(IService<int, Cita> citaService, Cita cita, Action closeAction, bool isNew)
    {
        _citaService = citaService;
        _citaOriginal = cita;
        _closeAction = closeAction;
        _isNew = isNew;
        
        if (!_isNew)
        {
            _logger.Debug("Cargando datos en el formulario de edición para la cita ID: {Id}", cita.Id);
            
            Titulo = "Edición Cita";
            Matricula = cita.Matricula;
            Marca = cita.Marca;
            Modelo = cita.Modelo;
            Dni = cita.DniDueño;
            Cilindrada = cita.Cilindrada.ToString(_invariant);
            MotorSeleccionado = cita.Motor.ToString();
            FechaMatriculacion = cita.FechaMatriculacion;
            FechaInspeccion = cita.FechaInspeccion;
        }
        else
        {
            _logger.Debug("Abriendo formulario en modo creación de nueva cita.");
            Titulo = "Crear Cita";
        }
    }
    
    [RelayCommand]
    private void GuardarCambios()
    {
        if (_isNew) Crear();
        else Actualizar();
    }
    
    private void Crear()
    {
        _logger.Information("Iniciando el proceso de creación de una nueva cita.");
        _logger.Debug("Datos capturados de la interfaz - Matrícula: {Matricula}, Motor: {Motor}, F.Matriculacion: {FMat}, F.Inspeccion: {FInsp}", 
             Matricula, MotorSeleccionado, FechaMatriculacion?.ToString(_isoFormat, _invariant), FechaInspeccion?.ToString(_isoFormat, _invariant));
        
         var nuevo = new CitaDto() with
         {
             Matricula = this.Matricula,
             Marca = this.Marca,
             Modelo = this.Modelo,
             DniDueño = this.Dni,
             Cilindrada = double.TryParse(this.Cilindrada, out var c) ? c : 0,
             Motor = Enum.TryParse<Motor>(MotorSeleccionado, out var m) ? (int)m : (int)Motor.Gasolina,
             FechaMatriculacion = this.FechaMatriculacion?.ToString(_isoFormat, _invariant) ?? DateTime.Today.ToString(_isoFormat, _invariant),
             FechaInspeccion = this.FechaInspeccion?.ToString(_isoFormat, _invariant) ?? DateTime.Today.ToString(_isoFormat, _invariant)
         };
     
         _logger.Debug("Enviando nueva cita al servicio para su inserción...");
         var result = _citaService.Agregar(nuevo.ToModel());
     
         if (result.IsSuccess)
         {
             _logger.Information("La cita se ha guardado con éxito en la base de datos. Cerrando ventana.");
             MessageBox.Show("La cita se ha creado correctamente.", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
             
             _closeAction();
         }
         else 
         {
             _logger.Error("Error al intentar registrar la nueva cita. Motivo: {Error}", result.Error);
             MessageBox.Show($"No se pudo crear la cita: {result.Error}");
         }
    }
    
    private void Actualizar()
    {
        _logger.Information("Iniciando el proceso de guardado de cambios para la cita ID: {Id}", _citaOriginal.Id);

        _logger.Debug("Datos capturados de la interfaz - Matrícula: {Matricula}, Motor: {Motor}, F.Matriculacion: {FMat}, F.Inspeccion: {FInsp}", 
                Matricula, MotorSeleccionado, FechaMatriculacion?.ToString(_isoFormat, _invariant), FechaInspeccion?.ToString(_isoFormat, _invariant));

        var nuevo = _citaOriginal.ToDto() with
        {
            Matricula = this.Matricula,
            Marca = this.Marca,
            Modelo = this.Modelo,
            DniDueño = this.Dni,
            Cilindrada = double.TryParse(this.Cilindrada, out var c) ? c : 0,
            Motor = Enum.TryParse<Motor>(MotorSeleccionado, out var m) ? (int)m : (int)Motor.Gasolina,
            FechaMatriculacion = this.FechaMatriculacion?.ToString(_isoFormat, _invariant) ?? DateTime.Today.ToString(_isoFormat, _invariant),
            FechaInspeccion = this.FechaInspeccion?.ToString(_isoFormat, _invariant) ?? DateTime.Today.ToString(_isoFormat, _invariant)
        };

            _logger.Debug("Enviando datos actualizados...");
            var result = _citaService.Actualizar(_citaOriginal.Id, nuevo.ToModel()); 

            if (result.IsSuccess)
            {
                _logger.Information("La base de datos se actualizó con éxito para el ID: {Id}. Cerrando ventana.", _citaOriginal.Id);
                MessageBox.Show("La cita se ha actualizado correctamente.", "Operación Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                _closeAction();
            }
            else 
            {
                _logger.Error("Error al intentar actualizar la cita ID {Id} en el servicio. Motivo: {Error}", _citaOriginal.Id, result.Error);
                MessageBox.Show($"No se pudo guardar la cita: {result.Error}");
            }
        }

    [RelayCommand]
    private void Cancelar()
    {
        _logger.Information("El usuario ha cancelado la edición de la cita ID: {Id}. Cerrando ventana.", _citaOriginal.Id);
        _closeAction();
    }
}