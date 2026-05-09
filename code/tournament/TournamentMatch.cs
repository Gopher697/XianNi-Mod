namespace xn.tournament
{
    public static class TournamentMatch
    {
        private const float DEFEAT_HP_THRESHOLD = 0.1f; 
        private const float TANTRUM_DURATION = 300f;    
        private const float MATCH_TIMEOUT = 60f;        
        private static string T(string key, string fallback, params object[] args)
        {
            string text = LocalizedTextManager.getText(key);
            if (string.IsNullOrEmpty(text) || text == key) text = fallback;
            return args != null && args.Length > 0 ? string.Format(text, args) : text;
        }
        public static void StartMatch(MatchData match)
        {
            if (match == null) return;
            TournamentManager.ClearDeathTriggers(match);
            var f1 = TournamentManager.GetActorById(match.Fighter1Id);
            var f2 = TournamentManager.GetActorById(match.Fighter2Id);
            if (f1 == null || !f1.isAlive())
            {
                match.WinnerId = match.Fighter2Id;
                match.LoserId = match.Fighter1Id;
                match.IsFinished = true;
                return;
            }
            if (f2 == null || !f2.isAlive())
            {
                match.WinnerId = match.Fighter1Id;
                match.LoserId = match.Fighter2Id;
                match.IsFinished = true;
                return;
            }
            match.IsDeathMatch = xn.config.ModConfigHooks.EnableTournamentDeathMatch
                && UnityEngine.Random.Range(0f, 1f) < 0.2f;
            if (match.IsDeathMatch)
            {
                xn.world.BroadcastSystem.Custom(T("broadcast_tournament_deathmatch", "Death duel! {0} VS {1} - no retreat!", f1.getName(), f2.getName()));
            }
            f1.cancelAllBeh();
            f2.cancelAllBeh();
            xn.access.ActorAccess.GetData(f1).health = f1.getMaxHealth();
            xn.access.ActorAccess.GetData(f2).health = f2.getMaxHealth();
            xn.access.BaseSimObjectAccess.AddStatusEffect(f1, "tantrum", TANTRUM_DURATION);
            xn.access.BaseSimObjectAccess.AddStatusEffect(f2, "tantrum", TANTRUM_DURATION);
            f1.startFightingWith(f2);
            f2.startFightingWith(f1);
            match.StartTime = UnityEngine.Time.time;
        }
        public static void UpdateMatch(MatchData match)
        {
            if (match == null || match.IsFinished) return;
            var f1 = TournamentManager.GetActorById(match.Fighter1Id);
            var f2 = TournamentManager.GetActorById(match.Fighter2Id);
            bool f1Alive = f1 != null && f1.isAlive();
            bool f2Alive = f2 != null && f2.isAlive();
            if (!f1Alive && !f2Alive)
            {
                match.WinnerId = null;
                match.LoserId = null;
                match.IsFinished = true;
                xn.world.BroadcastSystem.Custom(T("broadcast_tournament_double_ko", "Both tournament fighters fell together!"));
                return;
            }
            if (!f1Alive)
            {
                match.WinnerId = match.Fighter2Id;
                match.LoserId = match.Fighter1Id;
                match.IsFinished = true;
                StopFightAndHeal(f2);
                BroadcastWinner(f2);
                return;
            }
            if (!f2Alive)
            {
                match.WinnerId = match.Fighter1Id;
                match.LoserId = match.Fighter2Id;
                match.IsFinished = true;
                StopFightAndHeal(f1);
                BroadcastWinner(f1);
                return;
            }
            float elapsed = UnityEngine.Time.time - match.StartTime;
            if (elapsed >= MATCH_TIMEOUT)
            {
                int f1Hp = xn.access.ActorAccess.GetData(f1).health;
                int f2Hp = xn.access.ActorAccess.GetData(f2).health;
                bool f1Wins;
                string reason;
                if (f1Hp > f2Hp)
                {
                    f1Wins = true;
                    reason = T("tournament_reason_hp_advantage", "health advantage");
                }
                else if (f2Hp > f1Hp)
                {
                    f1Wins = false;
                    reason = T("tournament_reason_hp_advantage", "health advantage");
                }
                else
                {
                    f1Wins = UnityEngine.Random.Range(0, 2) == 0;
                    reason = T("tournament_reason_random", "random judgment");
                }
                if (f1Wins)
                {
                    match.WinnerId = match.Fighter1Id;
                    match.LoserId = match.Fighter2Id;
                }
                else
                {
                    match.WinnerId = match.Fighter2Id;
                    match.LoserId = match.Fighter1Id;
                }
                match.IsFinished = true;
                StopFightAndHeal(f1);
                StopFightAndHeal(f2);
                var winner = f1Wins ? f1 : f2;
                xn.world.BroadcastSystem.PostActor(winner, T("broadcast_tournament_timeout_win", "Match timeout! {0} won by {1}!", winner.getName(), reason));
                return;
            }
            if (match.IsDeathMatch)
            {
                EnsureFighting(f1, f2);
                EnsureFighting(f2, f1);
                EnsureStayInArena(f1);
                EnsureStayInArena(f2);
                return;
            }
            string deathLoser = TournamentManager.CheckDeathTriggerLoser(match);
            if (!string.IsNullOrEmpty(deathLoser))
            {
                if (deathLoser == match.Fighter1Id)
                {
                    match.WinnerId = match.Fighter2Id;
                    match.LoserId = match.Fighter1Id;
                }
                else
                {
                    match.WinnerId = match.Fighter1Id;
                    match.LoserId = match.Fighter2Id;
                }
                match.IsFinished = true;
                TournamentManager.ClearDeathTriggers(match);
                StopFightAndHeal(f1);
                StopFightAndHeal(f2);
                var loser = deathLoser == match.Fighter1Id ? f1 : f2;
                var winner = deathLoser == match.Fighter1Id ? f2 : f1;
                xn.world.BroadcastSystem.PostActor(loser, T("broadcast_tournament_near_death_defeat", "{0} was defeated near death!", loser.getName()));
                BroadcastWinner(winner);
                return;
            }
            float hp1 = (float)xn.access.ActorAccess.GetData(f1).health / f1.getMaxHealth();
            float hp2 = (float)xn.access.ActorAccess.GetData(f2).health / f2.getMaxHealth();
            if (hp1 <= DEFEAT_HP_THRESHOLD)
            {
                match.WinnerId = match.Fighter2Id;
                match.LoserId = match.Fighter1Id;
                match.IsFinished = true;
                StopFightAndHeal(f1);
                StopFightAndHeal(f2);
                BroadcastWinner(f2);
                return;
            }
            if (hp2 <= DEFEAT_HP_THRESHOLD)
            {
                match.WinnerId = match.Fighter1Id;
                match.LoserId = match.Fighter2Id;
                match.IsFinished = true;
                StopFightAndHeal(f1);
                StopFightAndHeal(f2);
                BroadcastWinner(f1);
                return;
            }
            EnsureFighting(f1, f2);
            EnsureFighting(f2, f1);
            EnsureStayInArena(f1);
            EnsureStayInArena(f2);
        }
        private static void StopFightAndHeal(Actor actor)
        {
            if (actor != null && actor.isAlive())
            {
                actor.cancelAllBeh();
                actor.clearAttackTarget();
                actor.finishStatusEffect("tantrum");
                xn.access.ActorAccess.GetData(actor).health = actor.getMaxHealth();
            }
        }
        private static void BroadcastWinner(Actor winner)
        {
            if (winner != null)
            {
                xn.world.BroadcastSystem.PostActor(winner, T("broadcast_tournament_winner", "{0} won!", winner.getName()));
            }
        }
        private static void EnsureFighting(Actor attacker, Actor target)
        {
            if (attacker == null || target == null) return;
            if (!attacker.isAlive() || !target.isAlive()) return;
            if (!xn.access.BaseSimObjectAccess.HasStatus(attacker, "tantrum"))
            {
                xn.access.BaseSimObjectAccess.AddStatusEffect(attacker, "tantrum", TANTRUM_DURATION);
            }
            if (!xn.access.ActorAccess.HasAttackTarget(attacker))
            {
                attacker.startFightingWith(target);
            }
        }
        private static void EnsureStayInArena(Actor actor)
        {
            if (actor == null || !actor.isAlive()) return;
            var currentTile = actor.current_tile;
            if (currentTile == null) return;
            if (!TournamentArena.IsInArena(currentTile))
            {
                var centerTile = TournamentManager.GetArenaCenterTile();
                if (centerTile != null)
                {
                    actor.cancelAllBeh();
                    xn.access.ActorAccess.SpawnOn(actor, centerTile);
                }
            }
        }
    }
}
