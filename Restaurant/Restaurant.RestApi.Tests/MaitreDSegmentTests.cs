﻿/* Copyright (c) Mark Seemann 2020. All rights reserved. */
using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Ploeh.Samples.Restaurants.RestApi.Tests
{
    public sealed class MaitreDSegmentTests
    {
        [Property]
        public Property Segment()
        {
            return Prop.ForAll(
                (from rs in Gens.Reservations
                 from m in Gens.MaitreD(rs)
                 from d in GenDate(rs)
                 select (m, d, rs)).ToArbitrary(),
                t => SegmentImp(t.m, t.d, t.rs));
        }

        private static void SegmentImp(
            MaitreD sut,
            DateTime date,
            Reservation[] reservations)
        {
            var actual = sut.Segment(date, reservations);

            Assert.NotEmpty(actual);
            Assert.Equal(
                date.Date.Add((TimeSpan)sut.OpensAt),
                actual.First().At);
            Assert.Equal(
                date.Date.Add((TimeSpan)sut.LastSeating),
                actual.Last().At);
            AssertFifteenMinuteDistances(actual);
            Assert.All(actual, ts => AssertTables(sut.Tables, ts.Tables));
            Assert.All(
                actual,
                ts => AssertRelevance(reservations, sut.SeatingDuration, ts));
        }

        private static void AssertFifteenMinuteDistances(
            IEnumerable<TimeSlot> actual)
        {
            var times = actual.Select(ts => ts.At).OrderBy(t => t);
            var deltas = times.Zip(times.Skip(1), (x, y) => y - x);
            Assert.All(deltas, d => Assert.Equal(TimeSpan.FromMinutes(15), d));
        }

        private static void AssertTables(
           IEnumerable<Table> expected,
           IEnumerable<Table> actual)
        {
            Assert.Equal(expected.Count(), actual.Count());
            Assert.Equal(
                expected.Sum(t => t.Capacity),
                actual.Sum(t => t.Capacity));
        }

        private static void AssertRelevance(
            IEnumerable<Reservation> reservations,
            TimeSpan seatingDuration,
            TimeSlot timeSlot)
        {
            var seating = new Seating(seatingDuration, timeSlot.At);
            var expected = reservations
                .Select(r => (new Seating(seatingDuration, r.At), r))
                .Where(t => seating.Overlaps(t.Item1))
                .Select(t => t.r)
                .ToHashSet();

            var actual = timeSlot.Tables
                .SelectMany(t => t.Accept(new ReservationsVisitor()))
                .ToHashSet();

            Assert.True(
                expected.SetEquals(actual),
                $"Expected: {expected}; actual {actual}.");
        }

        private sealed class ReservationsVisitor :
            ITableVisitor<IEnumerable<Reservation>>
        {
            public IEnumerable<Reservation> VisitCommunal(
                int seats,
                IReadOnlyCollection<Reservation> reservations)
            {
                return reservations;
            }

            public IEnumerable<Reservation> VisitStandard(
                int seats,
                Reservation? reservation)
            {
                if (reservation is { })
                    yield return reservation;
            }
        }

        /// <summary>
        /// Generate either an unconstrained, random date, or one picked from
        /// one of the input <paramref name="reservations" />.
        /// </summary>
        /// <param name="reservations">
        /// The reservations from which a date might be picked.
        /// </param>
        /// <returns>
        /// A generator that may return a date among the supplied
        /// <paramref name="reservations" />. If so, the reservation is picked
        /// at random. If the collection of reservations is empy, a random date
        /// is returned. This may also be the case even if the reservation
        /// collection is non-empty. The chance of that is 50%.
        /// </returns>
        private static Gen<DateTime> GenDate(
            IEnumerable<Reservation> reservations)
        {
            var randomDayGen = Arb.Default.DateTime().Generator;
            if (!reservations.Any())
                return randomDayGen;

            var oneOfReservationsDayGet = Gen.Elements(reservations
                .Select(r => r.At));

            return Gen.OneOf(randomDayGen, oneOfReservationsDayGet);
        }
    }
}
