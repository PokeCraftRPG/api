using PokeGame.Core.Search;

namespace PokeGame.Core.Trainers.Models;

public record SearchTrainersPayload : SearchPayload<TrainerSort>; // TODO(fpion): filters
