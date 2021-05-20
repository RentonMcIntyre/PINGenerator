import { TestBed } from '@angular/core/testing';

import { SupabaseHelperService } from './supabase-helper.service';

describe('SupabaseHelperService', () => {
  let service: SupabaseHelperService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(SupabaseHelperService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
