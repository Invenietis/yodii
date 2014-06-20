using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public interface ITimerService : IYodiiService
    {
        void IncreaseSpeed();

        void DecreaseSpeed();

        void Stop();

        void Start();

        //ici va falloir ajouter de quoi s'inscrire à l'event du timer, sinon ca sert pas à grd chose.
    }
}
