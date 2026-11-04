import { describe, expect, it } from 'vitest';
import { asCalendarDate } from './pantry-date';

describe('Pantry date display', () => {
  it('preserves the calendar day from an ISO date without timezone shifting', () => {
    const date = asCalendarDate('2026-11-04T00:00:00Z');

    expect(date).not.toBeNull();
    expect(date?.getFullYear()).toBe(2026);
    expect(date?.getMonth()).toBe(10);
    expect(date?.getDate()).toBe(4);
  });

  it('returns no date when the optional expiry is missing or invalid', () => {
    expect(asCalendarDate(null)).toBeNull();
    expect(asCalendarDate('not-a-date')).toBeNull();
  });
});
