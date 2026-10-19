import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';
import { SpecRangePipe, StatusLabelPipe } from './shared/shared';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('should render the navigation', async () => {
    const fixture = TestBed.createComponent(App);
    await fixture.whenStable();
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('.brand')?.textContent).toContain('LabTrack');
    expect(el.querySelectorAll('nav a').length).toBe(2);
  });
});

describe('Pipes', () => {
  it('translates status labels', () => {
    const pipe = new StatusLabelPipe();
    expect(pipe.transform('Approved')).toBe('Aprovada');
    expect(pipe.transform('Rejected')).toBe('Reprovada');
    expect(pipe.transform('Pending')).toBe('Pendente');
  });

  it('formats specification ranges', () => {
    const pipe = new SpecRangePipe();
    expect(pipe.transform(6.5, 7.5)).toBe('6,5 – 7,5');
    expect(pipe.transform(null, 50)).toBe('≤ 50');
    expect(pipe.transform(99.5, null)).toBe('≥ 99,5');
    expect(pipe.transform(null, null)).toBe('—');
  });
});
