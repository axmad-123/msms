import { useCallback, useEffect, useState, type ChangeEvent, type FormEvent } from 'react';
import { Camera, ChevronLeft, ChevronRight, Edit, Eye, Plus, Search, Trash2, User } from 'lucide-react';
import { msmsApi } from '../../api/msmsApi';
import { getErrorMessage } from '../../api/client';
import type { ClassItem, StudentDetails, StudentItem } from '../../types';
import { GENDER_LABELS, STUDENT_STATUS } from '../../types';
import { matchesSearch } from '../../utils/search';
import {
  Alert, Badge, Button, Card, CardBody, CardHeader, EmptyState, Input, Modal,
  PageHeader, Select, TableWrap, Td, Th,
} from '../../components/ui';

const PAGE_OPTIONS = [
  { label: '10', value: 10 },
  { label: '50', value: 50 },
  { label: '100', value: 100 },
  { label: 'All', value: 0 },
] as const;

const emptyForm = {
  email: '', password: '', fullName: '', studentNumber: '', firstName: '', lastName: '',
  dateOfBirth: '', placeOfBirth: '', gender: '', classId: '', photoUrl: '', monthlyFeeAmount: '',
};

export function StudentsPage() {
  const [students, setStudents] = useState<StudentItem[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(50);
  const [search, setSearch] = useState('');
  const [classes, setClasses] = useState<ClassItem[]>([]);
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<StudentItem | null>(null);
  const [profile, setProfile] = useState<StudentDetails | null>(null);
  const [form, setForm] = useState(emptyForm);
  const [feePreview, setFeePreview] = useState<number | null>(null);
  const [photoFile, setPhotoFile] = useState<File | null>(null);
  const [photoPreview, setPhotoPreview] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const load = useCallback(() => {
    const request = search.trim()
      ? msmsApi.people.listAllStudents()
      : msmsApi.people.listStudents(page, pageSize);

    request.then((r) => {
      setStudents(r.data.items);
      setTotal(r.data.totalCount);
    });
    msmsApi.school.listClasses().then((r) => setClasses(r.data));
  }, [page, pageSize, search]);

  useEffect(() => { load(); }, [load]);

  useEffect(() => {
    if (!form.classId) {
      setFeePreview(null);
      return;
    }
    msmsApi.people.feePreview(form.classId).then((r) => {
      setFeePreview(r.data);
      if (!editing && !form.monthlyFeeAmount) {
        setForm((current) => ({ ...current, monthlyFeeAmount: String(r.data) }));
      }
    }).catch(() => setFeePreview(null));
  }, [form.classId]);

  const totalPages = pageSize <= 0 ? 1 : Math.max(1, Math.ceil(total / pageSize));
  const filteredStudents = students.filter((s) =>
    matchesSearch(search, [
      s.studentNumber,
      s.firstName,
      s.lastName,
      `${s.firstName} ${s.lastName}`,
      s.className,
      s.gradeLevel,
      s.gender != null ? GENDER_LABELS[s.gender] : '',
      STUDENT_STATUS[s.status],
    ])
  );

  const onPhotoChange = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setPhotoFile(file);
    setPhotoPreview(URL.createObjectURL(file));
  };

  const openCreate = () => {
    setEditing(null);
    setForm(emptyForm);
    setPhotoPreview(null);
    setPhotoFile(null);
    setOpen(true);
    setSuccessMsg('');
  };

  const openEdit = async (student: StudentItem) => {
    setError('');
    try {
      const res = await msmsApi.people.getStudent(student.id);
      const s = res.data;
      setEditing(student);
      setForm({
        email: s.email ?? '',
        password: '',
        fullName: `${s.firstName} ${s.lastName}`,
        studentNumber: s.studentNumber,
        firstName: s.firstName,
        lastName: s.lastName,
        dateOfBirth: s.dateOfBirth ?? '',
        placeOfBirth: s.placeOfBirth ?? '',
        gender: s.gender == null ? '' : String(s.gender),
        classId: s.classId ?? '',
        photoUrl: s.photoUrl ?? '',
        monthlyFeeAmount: '',
      });
      setPhotoPreview(s.photoUrl);
      setPhotoFile(null);
      setOpen(true);
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };

  const openProfile = async (id: string) => {
    setError('');
    try {
      const res = await msmsApi.people.getStudent(id);
      setProfile(res.data);
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };

  const remove = async (student: StudentItem) => {
    if (!confirm(`Delete ${student.firstName} ${student.lastName}?`)) return;
    setError('');
    try {
      await msmsApi.people.deleteStudent(student.id);
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    }
  };

  const submit = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    setSuccessMsg('');
    try {
      let photoUrl = form.photoUrl || null;
      if (photoFile) {
        const up = await msmsApi.people.uploadPhoto(photoFile);
        photoUrl = up.data.url;
      }
      if (editing) {
        await msmsApi.people.updateStudent(editing.id, {
          firstName: form.firstName,
          lastName: form.lastName,
          dateOfBirth: form.dateOfBirth || null,
          placeOfBirth: form.placeOfBirth || null,
          gender: form.gender === '' ? null : Number(form.gender),
          photoUrl,
          classId: form.classId || null,
          status: editing.status,
        });
        setSuccessMsg('Student updated.');
      } else {
        const result = await msmsApi.people.createStudent({
          ...form,
          dateOfBirth: form.dateOfBirth || null,
          placeOfBirth: form.placeOfBirth || null,
          gender: form.gender === '' ? null : Number(form.gender),
          photoUrl,
          classId: form.classId || null,
          monthlyFeeAmount: Number(form.monthlyFeeAmount),
        });
        setSuccessMsg(
          `Student registered. Monthly fee: $${result.data.monthlyFeeAmount.toFixed(2)} (${result.data.feeMonth}/${result.data.feeYear})`
        );
      }
      setOpen(false);
      setEditing(null);
      setForm(emptyForm);
      setPhotoFile(null);
      setPhotoPreview(null);
      load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <PageHeader
        title="Students"
        description="Professional registration with photo, gender, birthplace, and monthly fee"
        action={<Button onClick={openCreate}><Plus className="h-4 w-4" /> Register Student</Button>}
      />

      {successMsg && <div className="mb-4"><Alert message={successMsg} type="success" /></div>}
      {error && !open && <div className="mb-4"><Alert message={error} /></div>}

      <Card>
        <CardHeader
          title="All Students"
          subtitle={search ? `${filteredStudents.length} matched · ${total} total` : `${total} total`}
          action={
            <div className="flex flex-wrap gap-2">
              <label className="relative block">
                <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-slate-400" />
                <input
                  value={search}
                  onChange={(e) => { setSearch(e.target.value); setPage(1); }}
                  placeholder="Search name, ID, class..."
                  className="w-64 rounded-xl border border-slate-200 bg-slate-50/80 py-2.5 pl-9 pr-3 text-sm outline-none transition focus:border-indigo-500 focus:bg-white focus:ring-4 focus:ring-indigo-500/10"
                />
              </label>
              <Select
                label=""
                className="!mb-0 min-w-[120px]"
                value={String(pageSize)}
                onChange={(e) => { setPageSize(Number(e.target.value)); setPage(1); }}
              >
                {PAGE_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>Show {o.label}</option>
                ))}
              </Select>
            </div>
          }
        />
        <CardBody className="p-0">
          {filteredStudents.length === 0 ? (
            <EmptyState message="No students yet." />
          ) : (
            <TableWrap>
              <thead>
                <tr>
                  <Th>Photo</Th>
                  <Th>ID</Th>
                  <Th>Name</Th>
                  <Th>Gender</Th>
                  <Th>Class</Th>
                  <Th>Status</Th>
                  <Th>Actions</Th>
                </tr>
              </thead>
              <tbody>
                {filteredStudents.map((s) => (
                  <tr key={s.id} className="transition hover:bg-indigo-50/30">
                    <Td>
                      {s.photoUrl ? (
                        <img src={s.photoUrl} alt="" className="h-10 w-10 rounded-full object-cover ring-2 ring-slate-100" />
                      ) : (
                        <span className="flex h-10 w-10 items-center justify-center rounded-full bg-slate-100 text-slate-400">
                          <User className="h-5 w-5" />
                        </span>
                      )}
                    </Td>
                    <Td>{s.studentNumber}</Td>
                    <Td className="font-medium">{s.firstName} {s.lastName}</Td>
                    <Td>{s.gender != null ? GENDER_LABELS[s.gender] : '—'}</Td>
                    <Td>{s.className ?? '—'}</Td>
                    <Td><Badge tone={s.status === 0 ? 'success' : 'default'}>{STUDENT_STATUS[s.status]}</Badge></Td>
                    <Td>
                      <div className="flex gap-2">
                        <Button size="sm" variant="secondary" onClick={() => openProfile(s.id)}><Eye className="h-4 w-4" /></Button>
                        <Button size="sm" variant="secondary" onClick={() => openEdit(s)}><Edit className="h-4 w-4" /></Button>
                        <Button size="sm" variant="danger" onClick={() => remove(s)}><Trash2 className="h-4 w-4" /></Button>
                      </div>
                    </Td>
                  </tr>
                ))}
              </tbody>
            </TableWrap>
          )}
        </CardBody>
        {!search && pageSize > 0 && totalPages > 1 && (
          <div className="flex items-center justify-between border-t border-slate-100 px-6 py-4">
            <p className="text-sm text-slate-500">Page {page} of {totalPages}</p>
            <div className="flex gap-2">
              <Button variant="secondary" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
                <ChevronLeft className="h-4 w-4" />
              </Button>
              <Button variant="secondary" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>
                <ChevronRight className="h-4 w-4" />
              </Button>
            </div>
          </div>
        )}
      </Card>

      <Modal open={open} onClose={() => setOpen(false)} title={editing ? 'Edit Student' : 'Register Student'} wide>
        {error && <Alert message={error} />}
        <form onSubmit={submit} className="grid gap-4 sm:grid-cols-2">
          <div className="sm:col-span-2 flex flex-col items-center gap-3 rounded-xl border border-dashed border-slate-200 bg-slate-50/50 p-6">
            {photoPreview ? (
              <img src={photoPreview} alt="Preview" className="h-28 w-28 rounded-2xl object-cover shadow-md" />
            ) : (
              <span className="flex h-28 w-28 items-center justify-center rounded-2xl bg-white text-slate-300 shadow-inner">
                <Camera className="h-12 w-12" />
              </span>
            )}
            <label className="cursor-pointer text-sm font-medium text-indigo-600 hover:text-indigo-700">
              Upload photo (Cloudinary)
              <input type="file" accept="image/*" className="hidden" onChange={onPhotoChange} />
            </label>
          </div>

          {!editing && (
            <>
              <Input label="Email" type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
              <Input label="Password" type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} required />
              <Input label="Full Name" className="sm:col-span-2" value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} required />
              <Input label="Student Number" value={form.studentNumber} onChange={(e) => setForm({ ...form, studentNumber: e.target.value })} required />
            </>
          )}
          <Input label="First Name" value={form.firstName} onChange={(e) => setForm({ ...form, firstName: e.target.value })} required />
          <Input label="Last Name" value={form.lastName} onChange={(e) => setForm({ ...form, lastName: e.target.value })} required />
          <Input label="Date of Birth" type="date" value={form.dateOfBirth} onChange={(e) => setForm({ ...form, dateOfBirth: e.target.value })} />
          <Input label="Place of Birth" value={form.placeOfBirth} onChange={(e) => setForm({ ...form, placeOfBirth: e.target.value })} />
          <Select label="Gender" value={form.gender} onChange={(e) => setForm({ ...form, gender: e.target.value })}>
            <option value="">— Select —</option>
            {Object.entries(GENDER_LABELS).map(([k, v]) => (
              <option key={k} value={k}>{v}</option>
            ))}
          </Select>
          <Select label="Class" value={form.classId} onChange={(e) => setForm({ ...form, classId: e.target.value })}>
            <option value="">— Select —</option>
            {classes.map((c) => (
              <option key={c.id} value={c.id}>{c.name} (Grade {c.gradeLevel})</option>
            ))}
          </Select>
          {!editing && (
            <Input
              label="Monthly fee amount"
              type="number"
              min="0"
              step="0.01"
              value={form.monthlyFeeAmount}
              onChange={(e) => setForm({ ...form, monthlyFeeAmount: e.target.value })}
              required
            />
          )}

          {feePreview != null && (
            <div className="sm:col-span-2 rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-800">
              <strong>Saved monthly fee:</strong> ${Number(form.monthlyFeeAmount || feePreview).toFixed(2)}. This is the amount that will be charged every month.
              <span className="mt-1 block text-emerald-700">Suggested for this class: ${feePreview.toFixed(2)}.</span>
            </div>
          )}

          <div className="sm:col-span-2 flex justify-end gap-2 pt-2">
            <Button type="button" variant="secondary" onClick={() => setOpen(false)}>Cancel</Button>
            <Button type="submit" loading={loading}>{editing ? 'Update' : 'Register & charge fee'}</Button>
          </div>
        </form>
      </Modal>

      <Modal open={!!profile} onClose={() => setProfile(null)} title="Student Profile" wide>
        {profile && (
          <div className="space-y-5">
            <div className="flex items-center gap-4 rounded-2xl bg-slate-50 p-4">
              {profile.photoUrl ? (
                <img src={profile.photoUrl} alt="" className="h-20 w-20 rounded-2xl object-cover" />
              ) : (
                <User className="h-14 w-14 text-slate-300" />
              )}
              <div>
                <h3 className="text-xl font-bold text-slate-900">{profile.firstName} {profile.lastName}</h3>
                <p className="font-mono text-sm text-indigo-600">{profile.studentNumber}</p>
                <p className="text-sm text-slate-500">{profile.email ?? 'No email'}</p>
              </div>
            </div>
            <div className="grid gap-3 sm:grid-cols-2">
              <Info label="Class" value={profile.className ?? '—'} />
              <Info label="Grade" value={profile.gradeLevel ?? '—'} />
              <Info label="Gender" value={profile.gender != null ? GENDER_LABELS[profile.gender] : '—'} />
              <Info label="Place of birth" value={profile.placeOfBirth ?? '—'} />
              <Info label="Date of birth" value={profile.dateOfBirth ?? '—'} />
              <Info label="Status" value={STUDENT_STATUS[profile.status] ?? String(profile.status)} />
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}

function Info({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-xl border border-slate-100 px-4 py-3">
      <p className="text-xs font-medium uppercase tracking-wide text-slate-400">{label}</p>
      <p className="mt-1 font-semibold text-slate-800">{value}</p>
    </div>
  );
}
